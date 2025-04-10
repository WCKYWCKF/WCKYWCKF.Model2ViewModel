using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

public record M2VMTypeInfo
{
    public M2VMTypeInfo()
    {
    }


    [SetsRequiredMembers]
    public M2VMTypeInfo(ITypeSymbol typeSymbol) : this(typeSymbol, CancellationToken.None)
    {
    }

    [SetsRequiredMembers]
    public M2VMTypeInfo(ITypeSymbol typeSymbol, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        GlobalTypeFullName = typeSymbol.ToDisplayString(M2VMHelper.GlobalSymbolDisplayFormat);
        NameSpace = typeSymbol.ContainingNamespace.ToDisplayString();
        Name = typeSymbol.Name;
        IsValue = typeSymbol.IsValueType;
        if (typeSymbol is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol)
            GenericTypes = namedTypeSymbol.TypeArguments
                .Select(symbol =>
                {
                    token.ThrowIfCancellationRequested();
                    return new MetadataAndFQType(symbol);
                })
                .ToList();
        else GenericTypes = [];

        MemberInfos = typeSymbol.GetMembers()
            .Where(x =>
            {
                token.ThrowIfCancellationRequested();
                return x is IPropertySymbol or IFieldSymbol;
            })
            .Where(x =>
            {
                token.ThrowIfCancellationRequested();
                return !M2VMHelper.IsAutoField(x as IFieldSymbol);
            })
            .Select(x =>
            {
                token.ThrowIfCancellationRequested();
                return new M2VMTypeMemberInfo(x);
            })
            .ToList();

        Interfaces = typeSymbol.AllInterfaces
            .Select(symbol =>
            {
                token.ThrowIfCancellationRequested();
                return new MetadataAndFQType(symbol);
            })
            .ToList();

        BaseType = typeSymbol.BaseType is null ? null : new MetadataAndFQType(typeSymbol.BaseType);
    }

    public required string GlobalTypeFullName { get; init; }
    public required string NameSpace { get; init; }
    public required string Name { get; init; }
    public bool IsValue { get; init; }
    public bool IsGeneric => GenericTypes.Count > 0;
    public required List<MetadataAndFQType> GenericTypes { get; init; }
    public required List<M2VMTypeMemberInfo> MemberInfos { get; init; }
    public required List<MetadataAndFQType> Interfaces { get; init; }
    public MetadataAndFQType? BaseType { get; init; }
}
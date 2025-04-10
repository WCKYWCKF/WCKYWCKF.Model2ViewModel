using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

public record M2VMTypeMemberInfo
{
    public M2VMTypeMemberInfo()
    {
    }

    [SetsRequiredMembers]
    public M2VMTypeMemberInfo(ISymbol symbol)
    {
        if (symbol is not (IFieldSymbol or IPropertySymbol))
            throw new ArgumentException("error type", nameof(symbol));
        var typeSymbol = (symbol as IFieldSymbol)?.Type ?? (symbol as IPropertySymbol)?.Type!;
        GlobalTypeFullName = typeSymbol.ToDisplayString(M2VMHelper.GlobalSymbolDisplayFormat);
        MemberName = symbol.Name;
        IsValue = typeSymbol.IsValueType;
        IsField = symbol is IFieldSymbol;
        Accessibility = symbol.DeclaredAccessibility;

        if (symbol is IPropertySymbol propertySymbol)
        {
            GetterAccessibility = propertySymbol.GetMethod?.DeclaredAccessibility;
            SetterAccessibility = propertySymbol.SetMethod?.DeclaredAccessibility;
        }
    }

    public required string GlobalTypeFullName { get; init; }
    public required bool IsValue { get; init; }
    public required string MemberName { get; init; }
    public required bool IsField { get; init; }
    public required Accessibility Accessibility { get; init; }
    public Accessibility? GetterAccessibility { get; init; }
    public Accessibility? SetterAccessibility { get; init; }
}
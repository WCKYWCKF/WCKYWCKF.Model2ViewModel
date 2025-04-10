using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

public record M2VMReplaceGenerationInfo : M2VMPropertyOrFieldOperationBase
{
    // public string? TypeArgumentRearrange { get; init; }

    public M2VMReplaceGenerationInfo()
    {
    }

    [SetsRequiredMembers]
    public M2VMReplaceGenerationInfo(AttributeData attributeData) : base(attributeData)
    {
        var typeSymbol = M2VMHelper.GetNamedArgument<ITypeSymbol>(attributeData, nameof(ReplaceFQType))!;
        ReplaceFQType = typeSymbol.ToDisplayString(M2VMHelper.GlobalSymbolDisplayFormat);
        IsValue = typeSymbol.IsValueType;
        ReplaceTypeTypeofInfo = new TypeofInfo(typeSymbol);
    }

    public required string ReplaceFQType { get; init; }
    public required bool IsValue { get; init; }
    public required TypeofInfo ReplaceTypeTypeofInfo { get; init; }
}
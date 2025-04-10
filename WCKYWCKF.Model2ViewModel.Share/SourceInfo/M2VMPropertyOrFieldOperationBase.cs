using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

public abstract record M2VMPropertyOrFieldOperationBase
{
    protected M2VMPropertyOrFieldOperationBase()
    {
    }

    [SetsRequiredMembers]
    protected M2VMPropertyOrFieldOperationBase(AttributeData attributeData)
    {
        var typeSymbol = M2VMHelper.GetNamedArgument<ITypeSymbol>(attributeData, nameof(TargetTypeFQType))!;
        TargetTypeFQType = typeSymbol.ToDisplayString(M2VMHelper.GlobalSymbolDisplayFormat);
        TargetTypeTypeofInfo = new TypeofInfo(typeSymbol);
        TargetMemberName = M2VMHelper.GetNamedArgument(attributeData, nameof(TargetMemberName), string.Empty)!;
    }

    public required string TargetTypeFQType { get; init; }
    public required TypeofInfo TargetTypeTypeofInfo { get; init; }
    public required string TargetMemberName { get; init; }
}
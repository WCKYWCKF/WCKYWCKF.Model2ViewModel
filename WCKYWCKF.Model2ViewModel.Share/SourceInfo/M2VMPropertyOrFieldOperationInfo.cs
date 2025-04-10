using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

public record M2VMPropertyOrFieldOperationInfo : M2VMPropertyOrFieldOperationBase
{
    public M2VMPropertyOrFieldOperationInfo()
    {
    }

    [SetsRequiredMembers]
    public M2VMPropertyOrFieldOperationInfo(AttributeData attributeData) : base(attributeData)
    {
        TargetOperation =
            M2VMHelper.GetNamedArgument<PropertyOrFieldOperationKind>(attributeData, nameof(TargetOperation));
    }

    public required PropertyOrFieldOperationKind TargetOperation { get; init; }
}
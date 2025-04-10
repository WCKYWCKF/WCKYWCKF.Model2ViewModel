using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

public sealed record TypeofInfo
{
    public TypeofInfo()
    {
    }

    [SetsRequiredMembers]
    public TypeofInfo(ITypeSymbol typeSymbol)
    {
        Sample = M2VMHelper.GetTypeofTargetTypeFQType(typeSymbol);
        FQType = M2VMHelper.GetTypeofTargetTypeFQType(typeSymbol, true);
    }

    public string? Sample { get; init; }
    public required string FQType { get; init; }
}
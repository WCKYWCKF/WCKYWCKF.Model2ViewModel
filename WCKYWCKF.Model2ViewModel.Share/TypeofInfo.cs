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

    public required string Sample { get; init; }
    public string? FQType { get; init; }
}
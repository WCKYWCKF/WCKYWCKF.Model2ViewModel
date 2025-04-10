using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

public record MetadataAndFQType
{
    public MetadataAndFQType()
    {
    }

    [SetsRequiredMembers]
    public MetadataAndFQType(ITypeSymbol typeSymbol)
    {
        GlobalTypeFullName = typeSymbol.ToDisplayString(M2VMHelper.GlobalSymbolDisplayFormat);
        Metadata = typeSymbol.MetadataName;
    }

    public required string GlobalTypeFullName { get; init; }
    public required string Metadata { get; init; }
}
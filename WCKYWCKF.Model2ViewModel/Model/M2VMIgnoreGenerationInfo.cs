using Microsoft.CodeAnalysis;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

internal record M2VMIgnoreGenerationInfo(
    string PropertyName,
    INamedTypeSymbol TargetModelType);
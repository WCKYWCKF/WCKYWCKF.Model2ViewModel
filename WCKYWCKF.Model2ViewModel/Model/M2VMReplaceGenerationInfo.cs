using Microsoft.CodeAnalysis;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

internal record M2VMReplaceGenerationInfo(
    string PropertyName,
    INamedTypeSymbol TargetModelType,
    INamedTypeSymbol ReplaceWithType);
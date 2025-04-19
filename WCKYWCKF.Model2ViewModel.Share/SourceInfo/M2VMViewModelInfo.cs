namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

public record M2VMViewModelInfo
{
    public required string NameSpace { get; init; }
    public required string ViewModelName { get; init; }
    public required List<PropertyBuildInfo> Properties { get; init; }
}
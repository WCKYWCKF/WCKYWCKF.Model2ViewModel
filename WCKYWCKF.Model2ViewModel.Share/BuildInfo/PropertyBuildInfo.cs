using static WCKYWCKF.RxUI.Model2ViewModel.Model.M2VMHelper;

namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

public record PropertyBuildInfo
{
    public required string PropertyName { get; init; }
    public required string GlobalPropertyFQType { get; init; }
    public required string GlobalSourceFQType { get; init; }
    public required bool IsNullable { get; init; }
    public required bool IeTypeReplace { get; init; }

    public string CreatePropertyCode(GenerateMode generateMode, bool useAutoField)
    {
        var fieldName = useAutoField ? "filed" : GetFieldName(PropertyName);
        var propertyType = GlobalPropertyFQType + (IsNullable ? "?" : "");
        return $$"""
                 {{(useAutoField ? "" : $"{TabStr}private {propertyType} {fieldName};")}} 
                 {{TabStr}}public {{propertyType}} {{PropertyName}} {
                 {{TabStr}}{{TabStr}}get => {{fieldName}};
                 {{TabStr}}{{TabStr}}set {
                 {{GetPropertySetterTemplate()}}
                 {{TabStr}}{{TabStr}}}
                 {{TabStr}}}
                 """ + (generateMode is GenerateMode.CommunityMvvm
            ? $$"""

                {{TabStr}}partial void On{{PropertyName}}Changing({{propertyType}} value);
                {{TabStr}}partial void On{{PropertyName}}Changing({{propertyType}} oldValue, {{propertyType}} newValue);
                {{TabStr}}partial void On{{PropertyName}}Changed({{propertyType}} value);
                {{TabStr}}partial void On{{PropertyName}}Changed({{propertyType}} oldValue, {{propertyType}} newValue);
                """
            : "");

        string GetPropertySetterTemplate()
        {
            return generateMode switch
            {
                GenerateMode.RxUI =>
                    $$"""{{TabStr}}{{TabStr}}{{TabStr}}this.RaiseAndSetIfChanged(ref {{{fieldName}}}, value);""",
                GenerateMode.CommunityMvvm =>
                    $$"""
                      {{TabStr}}{{TabStr}}{{TabStr}}if (!global::System.Collections.Generic.EqualityComparer<{{propertyType}}>.Default.Equals({{fieldName}}, value))
                      {{TabStr}}{{TabStr}}{{TabStr}}{
                      {{TabStr}}{{TabStr}}{{TabStr}}    var t = {{fieldName}};
                      {{TabStr}}{{TabStr}}{{TabStr}}    On{{PropertyName}}Changing(value);
                      {{TabStr}}{{TabStr}}{{TabStr}}    On{{PropertyName}}Changing(t, value);
                      {{TabStr}}{{TabStr}}{{TabStr}}    OnPropertyChanging();
                      {{TabStr}}{{TabStr}}{{TabStr}}    {{fieldName}} = value;
                      {{TabStr}}{{TabStr}}{{TabStr}}    On{{PropertyName}}Changed(value);
                      {{TabStr}}{{TabStr}}{{TabStr}}    On{{PropertyName}}Changed(t, value);
                      {{TabStr}}{{TabStr}}{{TabStr}}    OnPropertyChanged();
                      {{TabStr}}{{TabStr}}{{TabStr}}}
                      """,
                _ => $"{TabStr}{TabStr}{TabStr}SetField(ref {fieldName}, value);"
            };
        }
    }
}
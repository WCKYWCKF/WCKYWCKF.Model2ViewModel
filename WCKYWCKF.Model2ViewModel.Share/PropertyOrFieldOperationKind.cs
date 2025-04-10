namespace WCKYWCKF.RxUI.Model2ViewModel.Model;

[global::System.Flags]
public enum PropertyOrFieldOperationKind
{
    IgnoreProperty = 1,
    IncludePropertyOrField = 1 << 1,
    DoNotReplaceTargetType = 1 << 2,
    TypeIsNullable = 1 << 3,
    TypeIsNotNullable = 1 << 4
}
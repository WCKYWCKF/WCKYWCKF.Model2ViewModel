using WCKYWCKF.RxUI.Model2ViewModel.Model;

namespace WCKYWCKF.Model2ViewModel.Editor;

internal static class M2VMHelperEx
{
    public static string GetKey(this M2VMPropertyOrFieldOperationBase operationInfo)
    {
        return $"{operationInfo.TargetTypeFQType}->{operationInfo.TargetMemberName}";
    }

    public static string GetKey(this M2VMTypeMemberInfo memberInfo)
    {
        return $"{memberInfo.ContainerGlobalTypeFullName}->{memberInfo.MemberName}";
    }
}
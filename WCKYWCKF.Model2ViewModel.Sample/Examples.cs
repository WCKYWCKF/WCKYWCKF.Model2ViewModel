using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WCKYWCKF.Model2ViewModel.Sample;

public class ComplexModel
{
    private int TestPrivateField;
    public string TestPublicField;

    public List<Dictionary<string, int[]>>? NestedCollection { get; set; }
    public DDS? DDS { get; init; }
    public IEnumerable<DateTime>? Dates { get; }

    protected string TestProtected { get; init; }
    private int TestPrivate { get; init; }
}

public class DDS
{
    public string? Str { get; init; }
    public string? Str2 { get; set; }
    public List<int>? IntList { get; init; }
}

// [M2VMGenerationInfo(TargetTypeFQType = typeof(ClientCapabilities))]
[M2VMGenerationInfo(TargetTypeFQType = typeof(ComplexModel))]
[M2VMPropertyOrFieldOperationInfo(TargetTypeFQType = typeof(DDS),
    TargetMemberName = nameof(DDS.Str2),
    TargetOperation = PropertyOrFieldOperationKind.IgnoreProperty)]
[M2VMPropertyOrFieldOperationInfo(TargetTypeFQType = typeof(ComplexModel),
    TargetMemberName = nameof(ComplexModel.TestPublicField),
    TargetOperation = PropertyOrFieldOperationKind.IncludePropertyOrField)]
[M2VMPropertyOrFieldOperationInfo(TargetTypeFQType = typeof(ComplexModel),
    TargetMemberName = "TestPrivateField",
    TargetOperation = PropertyOrFieldOperationKind.IncludePropertyOrField |
                      PropertyOrFieldOperationKind.TypeIsNullable)]
[M2VMReplaceGenerationInfo(TargetTypeFQType = typeof(ComplexModel),
    TargetMemberName = nameof(ComplexModel.Dates),
    ReplaceFQType = typeof(List<string>))]
[M2VMSaveGenerationInfo(SaveFilePath = @"D:\Temp\test.json")]
public class TF : ObservableObject;
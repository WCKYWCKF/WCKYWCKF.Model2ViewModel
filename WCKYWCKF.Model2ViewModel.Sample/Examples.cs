using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using ReactiveUI;

namespace WCKYWCKF.Model2ViewModel.Sample;

public class ComplexModel
{
    private int? TestPrivateField;
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
[M2VMSaveGenerationInfo(SaveFilePath = @"D:\Temp\test.json")]
[M2VMUseAutoField]
public partial class TF;

[M2VMPropertyOrFieldOperationInfo(TargetTypeFQType = typeof(string), TargetMemberName = "TestPublicField", TargetOperation = PropertyOrFieldOperationKind.IncludePropertyOrField)]
[M2VMPropertyOrFieldOperationInfo(TargetTypeFQType = typeof(int), TargetMemberName = "TestPrivate", TargetOperation = PropertyOrFieldOperationKind.IncludePropertyOrField)]
[M2VMPropertyOrFieldOperationInfo(TargetTypeFQType = typeof(IEnumerable<DateTime>), TargetMemberName = "Dates", TargetOperation = PropertyOrFieldOperationKind.IncludePropertyOrField)]
[M2VMPropertyOrFieldOperationInfo(TargetTypeFQType = typeof(string), TargetMemberName = "TestProtected", TargetOperation = PropertyOrFieldOperationKind.IncludePropertyOrField)]
[M2VMPropertyOrFieldOperationInfo(TargetTypeFQType = typeof(string), TargetMemberName = "Str2", TargetOperation = PropertyOrFieldOperationKind.IgnoreProperty)]
[M2VMPropertyOrFieldOperationInfo(TargetTypeFQType = typeof(int), TargetMemberName = "TestPrivateField", TargetOperation = PropertyOrFieldOperationKind.IncludePropertyOrField | PropertyOrFieldOperationKind.TypeIsNullable)]
[M2VMReplaceGenerationInfo(TargetTypeFQType = typeof(IEnumerable<DateTime>), TargetMemberName = "Dates", ReplaceFQType = typeof(List<DateTime>))]
[M2VMReplaceGenerationInfo(TargetTypeFQType = typeof(string), TargetMemberName = "TestProtected", ReplaceFQType = typeof(double))]
public partial class TF;
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Common;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;
using ReactiveUI;
using WCKYWCKF.Model2ViewModel;

namespace WCKYWCKF.Model2ViewModel.Sample;

public partial class ComplexModel
{
    public List<Dictionary<string, int[]>>? NestedCollection { get; set; }
    public DDS? DDS { get; init; }
    public IEnumerable<DateTime>? Dates { get; }

    protected string TestProtected { get; init; }
    private string TestPrivate { get; init; }
    private string TestPrivateField;
    public string TestPublicField;

    public ComplexModel()
    {
    }
}

public class DDS
{
    public string? Str { get; init; }
    public string? Str2 { get; set; }
    public List<int>? IntList { get; init; }
}

[M2VMGenerationInfo(TargetTypeFQType = typeof(ClientCapabilities))]
[M2VMGenerationInfo(TargetTypeFQType = typeof(ComplexModel))]
[M2VMPropertyOrFieldOperationInfo(TargetTypeFQType = typeof(DDS),
    TargetMemberName = nameof(DDS.Str2),
    TargetOperation = PropertyOrFieldOperationKind.IgnoreProperty)]
[M2VMReplaceGenerationInfo(TargetTypeFQType = typeof(ComplexModel),
    TargetMemberName = nameof(ComplexModel.Dates),
    ReplaceFQType = typeof(List<string>))]
[M2VMSaveGenerationInfo(SaveFilePath = @"D:\Temp\test.json")]
public class TF : ObservableObject;
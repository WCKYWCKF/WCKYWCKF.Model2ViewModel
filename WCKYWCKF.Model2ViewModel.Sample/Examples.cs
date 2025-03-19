using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using ReactiveUI;
using WCKYWCKF.Model2ViewModel;

namespace WCKYWCKF.Model2ViewModel.Sample;

public partial class ComplexModel
{
    public List<Dictionary<string, int[]>>? NestedCollection { get; set; }
    public DDS? DDS { get; init; }
    public IEnumerable<DateTime> Dates { get; }
}

public class DDS
{
    public string? Str { get; init; }
    public string? Str2 { get; set; }
    public List<int> IntList { get; init; }
}

// [GenerateViewModel(ModelType = typeof(ClientCapabilities))]
[GenerateViewModel(ModelType = typeof(ComplexModel))]
[GenerateViewModelIgnore(ModelType = typeof(DDS), PropertyName = nameof(DDS.Str2))]
[GenerateViewModelReplace(ModelType = typeof(ComplexModel), PropertyName = nameof(ComplexModel.Dates), ReplaceWithType = typeof(List<string>))]
public class TF : ObservableObject;
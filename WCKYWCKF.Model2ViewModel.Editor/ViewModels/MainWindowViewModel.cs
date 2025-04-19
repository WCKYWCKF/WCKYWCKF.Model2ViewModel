using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Notifications;
using DynamicData;
using DynamicData.Binding;
using Irihi.Mantra.Markdown.Plugin.AvaloniaHybrid.MarkdigPlugins;
using Markdig;
using Markdig.Syntax;
using Microsoft.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using WCKYWCKF.RxUI.Model2ViewModel.Model;
using Notification = Ursa.Controls.Notification;

namespace WCKYWCKF.Model2ViewModel.Editor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public const string FilterTabItemMdText =
        """
        ### Model 元数据过滤器
        用于根据指定条件筛选 Model 元数据。
        :::
        该功能可用于以下批量操作场景：
        * 将所有实现 `IList` 接口的成员类型替换为可观察集合或可观察缓存
        * 批量设置特定成员的可为空性
        * 批量忽略或包含指定成员
        :::

        @@@


        ::: warning
        对与通过比对字符串进行过滤的，当输入的字符串在被比对的字符串中存在则通过。
        :::
        """;

//::: warning
//若需根据是否实现某个泛型接口来过滤 Model 元数据，必须使用接口的 `MetadataName` 形式。例如：
//- `IList<T>` 应写作 ```IList`1```
//:::

    public const string BatchEditingTabItemMdText =
        """
        ### Model 成员生成设置批量编辑器
        用于对选择的Model成员项进行批量的生成设置编辑。

        @@@

        ::: warning
        当出现以提问为主体的设置时：
        * 方框中是一条横杠时表示不启用此批量设置
        * 方框中为空时，表示否定。按下确认批量设置按钮后，所有选择的成员对应项都将被设置为否定。
        * 方框中为一个勾时，表示肯定。按下确认批量设置按钮后，所有选择的成员对应项都将被设置为肯定。
        :::
        """;

    public const string SummaryGeneratedBorderMdText =
        """
        ### M2VM 生成信息简介
        本次生成信息包含以下内容：
        - 生成模式：{0}
        - 生成属性字段模式：{1}
        - 生成所使用的根类型：`{2}`
        """;

    public const string RootTypePartialCodeBorderMdText =
        """
        #### 修改过的根类型装饰器集
        将下述代码覆盖到根类型上即可将编辑器中的修改应用到代码中。
        ``` csharp
        {0}
        ```
        ::: warning
        也许你不需要覆盖掉原来的根类型代码，将其注释掉即可？
        :::
        """;

    public const string InitialHelpBorderMdText =
        """
        ### 使用指南
        #### 在项目中加入 M2VM 的Nuget包。
        ```  shell
        dotnet add package WCKYWCKF.Model2ViewModel
        ```
        #### 生成Model元数据信息文件
        - 在项目中创建一个用于处理Model与ViewModel互转换的根类型并将你希望生成ViewModel版本的Model通过`M2VMGenerationInfo`标记它们。
        - 使用`M2VMSaveGenerationInfo`告诉M2VM的源生成器把Model的元数据信息存储到你指定的文件去。
           ::: warning
           根据编写M2VM源生成器时IDE与分析器给予的提示，不应该在源生成器中进行任何IO操作。但很显然，我没有那么多时间去研究Roslyn的API。
           这样不符合约定的事情可能会带来一些奇怪的影响（不会影响到使用此功能的解决方案的正确性与完整性），但通常你可以正常使用此功能。
           :::

        示例如下
        ``` csharp
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

        [M2VMGenerationInfo(TargetTypeFQType = typeof(ComplexModel))]
        [M2VMSaveGenerationInfo(SaveFilePath = @"D:\Temp\test.json")]
        public partial class TF : ObservableObject;
        ```

        ::: warning
        通常的，一个解决方案内应该只有一个用于处理M2VM的根类型。这是为了避免当M2VM生成的ViewModel过多时多个根类型生成重复的但内容不同的ViewModel。当然，你已经明确了此风险并确切需要这么做的话，谨慎一些即可。
        :::

        #### 打开元数据信息文件
        然后解放你的重复劳动的时间。
        """;

    public const string MemberAccessibilityDisplayTemplateKey = "MemberAccessibilityDisplayTemplate";
    public const string SelectionHandlingCheckBoxTemplateKey = "SelectionHandlingCheckBoxTemplate";
    public const string BuildInclusionCheckBoxTemplateKey = "BuildInclusionCheckBoxTemplate";
    public const string MemberBuildTypeDisplayTemplateKey = "MemberBuildTypeDisplayTemplate";
    public const string IsMemberNullableCheckBoxTemplateKey = "IsMemberNullableCheckBoxTemplate";

    private readonly IObservable<bool> _metadataIsNotEmpty;

    private readonly SourceCache<M2VMPropertyOrFieldOperationInfo, string> _operationInfos = new(y => y.GetKey());
    private readonly SourceCache<M2VMReplaceGenerationInfo, string> _replaceGenerationInfos = new(y => y.GetKey());
    private readonly ReadOnlyObservableCollection<MetadataTDGItemViewModel> _selection;

    static MainWindowViewModel()
    {
        MDPipeline = new MarkdownPipelineBuilder()
            .UseAvaloniaHybrid()
            .UseAlertBlocks()
            .UseAutoIdentifiers()
            .UseAbbreviations()
            .UsePreciseSourceLocation()
            .UseBootstrap()
            .UseAutoLinks()
            .UseCitations()
            .UseDiagrams()
            .UseGridTables()
            .UsePipeTables()
            .UseFigures()
            .UseFooters()
            .UseGlobalization()
            .UseAdvancedExtensions()
            .UseFootnotes()
            .UseEmphasisExtras()
            .UseGenericAttributes()
            .UseListExtras()
            .Build();
        FilterTabItemMdDocument = Markdown.Parse(FilterTabItemMdText, MDPipeline);
        BatchEditingTabItemMdDocument = Markdown.Parse(BatchEditingTabItemMdText, MDPipeline);
        InitialHelpBorderMdDocument = Markdown.Parse(InitialHelpBorderMdText, MDPipeline);
    }

    public MainWindowViewModel()
    {
        CompositeDisposable? disposable = null;
        var share = this.WhenAnyValue(x => x.ModelMetadata)
            .Select(m2VmGenerationInfo =>
            {
                disposable?.Dispose();
                var sourceList = new SourceCache<M2VMTypeInfo, string>(typeInfo => typeInfo.GlobalTypeFullName);
                if (m2VmGenerationInfo is not null)
                {
                    disposable = new CompositeDisposable();
                    sourceList.AddOrUpdate(m2VmGenerationInfo.TypeToGenerateAll);
                    _operationInfos.Clear();
                    _operationInfos.AddOrUpdate(m2VmGenerationInfo.Operations);
                    _replaceGenerationInfos.Clear();
                    _replaceGenerationInfos.AddOrUpdate(m2VmGenerationInfo.Replaces);

                    var propertyOrFieldOperationInfosShare = _operationInfos.Connect();
                    var replaceGenerationInfosShare = _replaceGenerationInfos.Connect();
                    propertyOrFieldOperationInfosShare.OnItemAdded(x => TryAdd(ModelMetadata!.Operations, x))
                        .Subscribe()
                        .DisposeWith(disposable);
                    propertyOrFieldOperationInfosShare.OnItemRemoved(x =>
                        ModelMetadata!.Operations.RemoveAll(y => y.GetKey() == x.GetKey())).Subscribe();
                    propertyOrFieldOperationInfosShare.OnItemUpdated((newValue, oldValue) =>
                        {
                            ModelMetadata!.Operations.RemoveAll(y => y.GetKey() == oldValue.GetKey());
                            TryAdd(ModelMetadata!.Operations, newValue);
                        })
                        .Subscribe()
                        .DisposeWith(disposable);
                    replaceGenerationInfosShare.OnItemAdded(x => TryAdd(ModelMetadata!.Replaces, x))
                        .Subscribe()
                        .DisposeWith(disposable);
                    replaceGenerationInfosShare
                        .OnItemRemoved(x => ModelMetadata!.Replaces.RemoveAll(y => y.GetKey() == x.GetKey()))
                        .Subscribe()
                        .DisposeWith(disposable);
                    replaceGenerationInfosShare.OnItemUpdated((newValue, oldValue) =>
                        {
                            ModelMetadata!.Replaces.RemoveAll(y => y.GetKey() == oldValue.GetKey());
                            TryAdd(ModelMetadata!.Replaces, newValue);
                        })
                        .Subscribe()
                        .DisposeWith(disposable);
                }

                return sourceList;
            })
            .Switch()
            .Transform(info =>
                new MetadataTDGItemViewModel(info, ModelMetadata!, _operationInfos, _replaceGenerationInfos, this))
            .Publish();

        share.TransformMany(x => x.FilterMembers, x => x.GetKey()!)
            .FilterOnObservable(x => x.WhenAnyValue(y => y.IsSelected))
            .Bind(out _selection)
            .Subscribe();

        share.FilterOnObservable(model => model.FilterMembers.WhenAnyValue(x => x.Count).Select(x => x > 0))
            .SortAndBind(out var source, SortExpressionComparer<MetadataTDGItemViewModel>.Ascending(x => x.Name))
            .Subscribe();
        Source = new HierarchicalTreeDataGridSource<MetadataTDGItemViewModel>(source)
        {
            Columns =
            {
                new TemplateColumn<MetadataTDGItemViewModel>(null, SelectionHandlingCheckBoxTemplateKey),
                new HierarchicalExpanderColumn<MetadataTDGItemViewModel>(
                    new TextColumn<MetadataTDGItemViewModel, string>("类型或成员名称", x => x.Name),
                    x => x.FilterMembers),
                new TextColumn<MetadataTDGItemViewModel, string>("成员种类",
                    x => x.IsField == null ? null : x.IsField == true ? "字段" : "属性"),
                new TemplateColumn<MetadataTDGItemViewModel>("可访问性", MemberAccessibilityDisplayTemplateKey),
                new TemplateColumn<MetadataTDGItemViewModel>("是否加入生成", BuildInclusionCheckBoxTemplateKey),
                new TemplateColumn<MetadataTDGItemViewModel>("是否可为空", IsMemberNullableCheckBoxTemplateKey),
                new TemplateColumn<MetadataTDGItemViewModel>("成员类型在生成中的替换", MemberBuildTypeDisplayTemplateKey),
                new TextColumn<MetadataTDGItemViewModel, string>("成员类型的完全限定名称", x => x.GlobalTypeFullName),
                new TextColumn<MetadataTDGItemViewModel, string>("成员类型在构建时类型的名称", x => x.MemberBuildType)
            }
        };
        share.TransformMany(x => x.Members, x => x.GetKey()!)
            .FilterOnObservable(x => x.WhenAnyValue(y => y.WeightedRatioForMemberCodeText).Select(y => y > 0),
                TimeSpan.FromSeconds(0.2))
            .FilterOnObservable(x =>
                this.WhenAnyValue(y => y.MinimumAcceptedWeightedRatio)
                    .CombineLatest(x.WhenAnyValue(y => y.WeightedRatioForMemberCodeText))
                    .Select(y => x.WeightedRatioForMemberCodeText >= y.First), TimeSpan.FromSeconds(0.2))
            .FilterOnObservable(x =>
                this.WhenAnyValue(y => y.Filter_MemberName)
                    .Select(y =>
                        string.IsNullOrWhiteSpace(y) || x.Name.Contains(y, StringComparison.CurrentCultureIgnoreCase)))
            .FilterOnObservable(x =>
                this.WhenAnyValue(y => y.Filter_IsField)
                    .Select(y => y is null || y == x.IsField))
            .FilterOnObservable(x =>
                this.WhenAnyValue(y => y.Filter_IsMemberIncludeInBuild)
                    .Select(y => y is null || y == x.IsMemberIncludedInBuild))
            .FilterOnObservable(x =>
                this.WhenAnyValue(y => y.Filter_MemberCanBeNull)
                    .Select(y => y is null || y == x.IsMemberNullable))
            .FilterOnObservable(x =>
                this.WhenAnyValue(y => y.Filter_MemberAccessibility)
                    .Select(y =>
                        y is FilterAccessibility.None || GetAccessibilityByFilterAccessibility(y) == x.Accessibility))
            .FilterOnObservable(x =>
                this.WhenAnyValue(y => y.Filter_MemberGetterAccessibility)
                    .Select(y =>
                        y is FilterAccessibility.None ||
                        GetAccessibilityByFilterAccessibility(y) == x.GetterAccessibility))
            .FilterOnObservable(x =>
                this.WhenAnyValue(y => y.Filter_MemberSetterAccessibility)
                    .Select(y =>
                        y is FilterAccessibility.None ||
                        GetAccessibilityByFilterAccessibility(y) == x.SetterAccessibility))
            .FilterOnObservable(x =>
                this.WhenAnyValue(y => y.Filter_MemberGlobeFQType)
                    .Select(y =>
                        string.IsNullOrWhiteSpace(y) ||
                        x.GlobalTypeFullName!.Contains(y, StringComparison.CurrentCultureIgnoreCase)))
            .FilterOnObservable(x =>
                this.WhenAnyValue(y => y.Filter_MemberBuildType)
                    .Select(y =>
                        string.IsNullOrWhiteSpace(y) ||
                        x.MemberBuildType!.Contains(y, StringComparison.CurrentCultureIgnoreCase)))
            .AutoRefreshOnObservable(x => x.WhenAnyValue(y => y.WeightedRatioForMemberCodeText))
            .SortAndBind(out var dataWarpers,
                SortExpressionComparer<MetadataTDGItemViewModel>
                    .Descending(x => x.WeightedRatioForMemberCodeText)
                    .ThenByAscending(x => x.Name))
            .Subscribe();
        SearchSource = new HierarchicalTreeDataGridSource<MetadataTDGItemViewModel>(dataWarpers)
        {
            Columns =
            {
                new TextColumn<MetadataTDGItemViewModel, string>("成员所处类型", x => x.ContainerGlobalTypeFullName),
                new HierarchicalExpanderColumn<MetadataTDGItemViewModel>(
                    new TextColumn<MetadataTDGItemViewModel, string>("成员名称", x => x.Name), x => x.Members),
                new TextColumn<MetadataTDGItemViewModel, string>("成员种类",
                    x => x.IsField == null ? null : x.IsField == true ? "字段" : "属性"),
                new TemplateColumn<MetadataTDGItemViewModel>("可访问性", MemberAccessibilityDisplayTemplateKey),
                new TemplateColumn<MetadataTDGItemViewModel>("是否加入生成", BuildInclusionCheckBoxTemplateKey),
                new TemplateColumn<MetadataTDGItemViewModel>("是否可为空", IsMemberNullableCheckBoxTemplateKey),
                new TemplateColumn<MetadataTDGItemViewModel>("成员类型在生成中的替换", MemberBuildTypeDisplayTemplateKey),
                new TextColumn<MetadataTDGItemViewModel, string>("成员类型的完全限定名称", x => x.GlobalTypeFullName),
                new TextColumn<MetadataTDGItemViewModel, string>("成员类型在构建时类型的名称", x => x.MemberBuildType)
            }
        };
        share.Connect();

        _metadataIsNotEmpty = this.WhenAnyValue(x => x.ModelMetadata).Select(x => x is not null);

        this.WhenAnyValue(x => x.Filter_MemberCodeText)
            .Do(value => { IsInSearchEditMode = !string.IsNullOrWhiteSpace(value); })
            .Subscribe();
        MinimumAcceptedWeightedRatio = 30;
        PreviewGeneratedCodeFiles = [];
        SummaryGeneratedBorderMdDocument = new MarkdownDocument();
        RootTypePartialCodeBorderMdDocument = new MarkdownDocument();
        return;

        void TryAdd<T>(List<T> target, T newItem)
        {
            if (!target.Contains(newItem))
                target.Add(newItem);
        }
    }

    [Reactive] public partial MarkdownDocument SummaryGeneratedBorderMdDocument { get; set; }
    [Reactive] public partial MarkdownDocument RootTypePartialCodeBorderMdDocument { get; set; }

    public static MarkdownDocument InitialHelpBorderMdDocument { get; }

    // public OperationLogViewModel OperationLogViewModel { get; } = new();
    public static MarkdownPipeline MDPipeline { get; }
    [Reactive] public partial string? Filter_MemberName { get; set; }
    [Reactive] public partial string? Filter_MemberGlobeFQType { get; set; }
    [Reactive] public partial string? Filter_MemberBuildType { get; set; }
    [Reactive] public partial string? BatchEditing_NewMemberBuildType { get; set; }
    [Reactive] public partial bool? BatchEditing_NewIsMemberIncludeInBuild { get; set; }
    [Reactive] public partial bool? BatchEditing_NewIsMemberNullable { get; set; }
    [Reactive] public partial bool? BatchEditing_NewIsMemberMarkedNonReplaceable { get; set; }
    [Reactive] public partial bool? Filter_IsField { get; set; }
    [Reactive] public partial FilterAccessibility Filter_MemberAccessibility { get; set; }
    [Reactive] public partial FilterAccessibility Filter_MemberGetterAccessibility { get; set; }
    [Reactive] public partial FilterAccessibility Filter_MemberSetterAccessibility { get; set; }
    [Reactive] public partial bool? Filter_IsMemberIncludeInBuild { get; set; }
    [Reactive] public partial bool? Filter_MemberCanBeNull { get; set; }
    [Reactive] public partial string? Filter_MemberCodeText { get; set; }
    [Reactive] public partial string? SaveFilePath { get; set; }
    [Reactive] public partial int MinimumAcceptedWeightedRatio { get; set; }

    public static MarkdownDocument FilterTabItemMdDocument { get; }
    public static MarkdownDocument BatchEditingTabItemMdDocument { get; }

    [Reactive] public partial bool IsInSearchEditMode { get; set; }
    [Reactive] public partial M2VMGenerationInfo? ModelMetadata { get; set; }

    // public HierarchicalTreeDataGridSource<MetadataTDGItemViewModel> FilterResult { get; }
    public HierarchicalTreeDataGridSource<MetadataTDGItemViewModel> Source { get; }
    public HierarchicalTreeDataGridSource<MetadataTDGItemViewModel> SearchSource { get; }
    [Reactive] public partial string? LoadingMessage { get; set; }

    [Reactive] public partial IReadOnlyList<ViewModelCodeFile>? PreviewGeneratedCodeFiles { get; set; }

    [ReactiveCommand(CanExecute = nameof(_metadataIsNotEmpty))]
    [RequiresDynamicCode(
        "Calls System.Text.Json.JsonSerializer.SerializeAsync<TValue>(Stream, TValue, JsonSerializerOptions, CancellationToken)")]
    [RequiresUnreferencedCode(
        "Calls System.Text.Json.JsonSerializer.SerializeAsync<TValue>(Stream, TValue, JsonSerializerOptions, CancellationToken)")]
    private async Task SaveChanged()
    {
        try
        {
            LoadingMessage = "正在保存Model元数据文件...";
            // await Task.Delay(TimeSpan.FromSeconds(2));
            await using var fileStream = File.OpenWrite(SaveFilePath!);
            await JsonSerializer.SerializeAsync(fileStream, M2VMHelperJSC.Default.M2VMGenerationInfo);
            Notification notification = new()
            {
                Title = "保存Model元数据文件成功",
                Content = $"文件已保存到：{SaveFilePath}",
                Type = NotificationType.Success
            };
            MessageBus.Current.SendMessage(notification);
        }
        catch (IOException e)
        {
            Notification notification = new()
            {
                Title = "打开Model元数据文件时失败",
                Content = $"{e}",
                Type = NotificationType.Error
            };
            MessageBus.Current.SendMessage(notification);
        }
        catch (JsonException e)
        {
            Notification notification = new()
            {
                Title = "序列化Model元数据文件时失败",
                Content = $"{e}",
                Type = NotificationType.Error
            };
            MessageBus.Current.SendMessage(notification);
        }
        catch (Exception e)
        {
            Notification notification = new()
            {
                Title = "保存Model元数据文件时发生了未知错误",
                Content = $"{e}",
                Type = NotificationType.Error
            };
            MessageBus.Current.SendMessage(notification);
        }
        finally
        {
            LoadingMessage = null;
            if (ModelMetadata is null)
                SaveFilePath = null;
        }
    }

    [ReactiveCommand(CanExecute = nameof(_metadataIsNotEmpty))]
    private async Task PreviewGeneratedCode()
    {
        if (ModelMetadata is null) return;
        PreviewGeneratedCodeFiles = null;
        LoadingMessage = "正在生成将要预览的代码...";
        var buildInfos = await Task.Run(() => ModelMetadata.CreateViewModelBuildInfos(out _));
        PreviewGeneratedCodeFiles = await Task.Run(() =>
            buildInfos.Select(x =>
                {
                    var code = x.CreateViewModelCode(ModelMetadata!.GenerateMode, ModelMetadata.UseAutoField);
                    return new ViewModelCodeFile
                    {
                        FileName = $"{x.Namespace}.{x.ViewModelName}",
                        Code = code,
                        CodeMdDocument = Markdown.Parse($"""
                                                         ``` csharp
                                                         {code}
                                                         ```
                                                         """, MDPipeline)
                    };
                })
                .OrderBy(x => x.FileName)
                .ToList());
        var operations = ModelMetadata.Operations.Select(x =>
        {
            return
                $"""[M2VMPropertyOrFieldOperationInfo(TargetTypeFQType = {x.TargetTypeTypeofInfo.Sample}, TargetMemberName = "{x.TargetMemberName}", TargetOperation = {GetOperation(x.TargetOperation)})]""";

            string GetOperation(PropertyOrFieldOperationKind operationKind)
            {
                string[] str =
                [
                    operationKind.HasFlag(PropertyOrFieldOperationKind.IgnoreProperty)
                        ? "PropertyOrFieldOperationKind.IgnoreProperty"
                        : "",
                    operationKind.HasFlag(PropertyOrFieldOperationKind.DoNotReplaceTargetType)
                        ? "PropertyOrFieldOperationKind.DoNotReplaceTargetType"
                        : "",
                    operationKind.HasFlag(PropertyOrFieldOperationKind.IncludePropertyOrField)
                        ? "PropertyOrFieldOperationKind.IncludePropertyOrField"
                        : "",
                    operationKind.HasFlag(PropertyOrFieldOperationKind.TypeIsNotNullable)
                        ? "PropertyOrFieldOperationKind.TypeIsNotNullable"
                        : "",
                    operationKind.HasFlag(PropertyOrFieldOperationKind.TypeIsNullable)
                        ? "PropertyOrFieldOperationKind.TypeIsNullable"
                        : ""
                ];
                StringBuilder builder = new();
                var distinct = str.Where(y => !string.IsNullOrEmpty(y)).ToList();
                builder.Append(distinct.FirstOrDefault());
                foreach (var item in distinct.Skip(1)) builder.Append(" | " + item);

                return builder.ToString();
            }
        });
        var replaces = ModelMetadata.Replaces.Select(x =>
            $"""[M2VMReplaceGenerationInfo(TargetTypeFQType = {x.TargetTypeTypeofInfo.Sample}, TargetMemberName = "{x.TargetMemberName}", ReplaceFQType = {x.ReplaceTypeTypeofInfo.Sample})]""");
        var rootCode = await Task.Run(() => $"""
                                             {string.Join("\n", operations)}
                                             {string.Join("\n", replaces)}
                                             public partial class {ModelMetadata.ClassName};
                                             """);
        SummaryGeneratedBorderMdDocument = await Task.Run(() => Markdown.Parse(
            string.Format(SummaryGeneratedBorderMdText,
                ModelMetadata.GenerateMode.ToString(),
                ModelMetadata.UseAutoField ? "使用field特性" : "使用字段",
                $"{ModelMetadata.NameSpace}.{ModelMetadata.ClassName}"), MDPipeline));
        RootTypePartialCodeBorderMdDocument = await Task.Run(() =>
            Markdown.Parse(string.Format(RootTypePartialCodeBorderMdText, rootCode), MDPipeline));
        LoadingMessage = null;
    }

    [ReactiveCommand(CanExecute = nameof(_metadataIsNotEmpty))]
    private void Restore()
    {
        if (ModelMetadata is null) return;

        if (_operationInfos.Count > 0)
            _operationInfos.Clear();
        if (_replaceGenerationInfos.Count > 0)
            _replaceGenerationInfos.Clear();
    }

    // [ReactiveCommand(CanExecute = nameof(_metadataIsNotEmpty))]
    // private bool ApplyFilter()
    // {
    //     return default;
    // }

    [ReactiveCommand(CanExecute = nameof(_metadataIsNotEmpty))]
    private void RestoreFilter()
    {
        Filter_MemberName = null;
        Filter_MemberGlobeFQType = null;
        Filter_MemberBuildType = null;
        Filter_IsField = null;
        Filter_MemberAccessibility = FilterAccessibility.None;
        Filter_MemberGetterAccessibility = FilterAccessibility.None;
        Filter_MemberSetterAccessibility = FilterAccessibility.None;
        Filter_IsMemberIncludeInBuild = null;
        Filter_MemberCanBeNull = null;
    }

    [ReactiveCommand]
    private void ConfirmBatchModify()
    {
        foreach (var metadataTdgItemViewModel in _selection.ToList())
        {
            if (BatchEditing_NewIsMemberIncludeInBuild is not null)
                metadataTdgItemViewModel.IsMemberIncludedInBuild = BatchEditing_NewIsMemberIncludeInBuild;
            if (BatchEditing_NewIsMemberMarkedNonReplaceable is not null)
                metadataTdgItemViewModel.IsMemberMarkedNonReplaceable = BatchEditing_NewIsMemberMarkedNonReplaceable;
            if (BatchEditing_NewIsMemberNullable is not null)
                metadataTdgItemViewModel.IsMemberNullable = BatchEditing_NewIsMemberNullable;
            if (BatchEditing_NewMemberBuildType is "null")
                metadataTdgItemViewModel.ConfirmChangesCommand.Execute("").Subscribe();
            else if (!string.IsNullOrWhiteSpace(BatchEditing_NewMemberBuildType))
                metadataTdgItemViewModel.ConfirmChangesCommand.Execute(BatchEditing_NewMemberBuildType).Subscribe();
        }
    }

    [ReactiveCommand]
    private void CancelCurrentSelection()
    {
        foreach (var metadataTdgItemViewModel in _selection)
            metadataTdgItemViewModel.IsSelected = false;
    }

    [ReactiveCommand]
    private async Task OpenModelMetadataFile(IReadOnlyList<string> readOnlyList)
    {
        string filePath;
        if (readOnlyList.Count > 0) filePath = readOnlyList[0];
        else return;
        try
        {
            LoadingMessage = "正在加载Model元数据文件...";
            // await Task.Delay(TimeSpan.FromSeconds(2));
            await using var fileStream = File.OpenRead(filePath);
            ModelMetadata = await JsonSerializer.DeserializeAsync(fileStream, M2VMHelperJSC.Default.M2VMGenerationInfo);
            SaveFilePath = filePath;
        }
        catch (IOException e)
        {
            Notification notification = new()
            {
                Title = "打开Model元数据文件时失败",
                Content = $"{e}",
                Type = NotificationType.Error
            };
            MessageBus.Current.SendMessage(notification);
        }
        catch (JsonException e)
        {
            Notification notification = new()
            {
                Title = "反序列化Model元数据文件时失败",
                Content = $"{e}",
                Type = NotificationType.Error
            };
            MessageBus.Current.SendMessage(notification);
        }
        catch (Exception e)
        {
            Notification notification = new()
            {
                Title = "加载Model元数据文件时发生了未知错误",
                Content = $"{e}",
                Type = NotificationType.Error
            };
            MessageBus.Current.SendMessage(notification);
        }
        finally
        {
            LoadingMessage = null;
            if (ModelMetadata is null)
                SaveFilePath = null;
        }
    }

    public static Accessibility? GetAccessibilityByFilterAccessibility(FilterAccessibility filterAccessibility)
    {
        return filterAccessibility switch
        {
            FilterAccessibility.Public => Accessibility.Public,
            FilterAccessibility.Internal => Accessibility.Internal,
            FilterAccessibility.Protected => Accessibility.Protected,
            FilterAccessibility.Private => Accessibility.Private,
            _ => null
        };
    }
}

public enum FilterAccessibility
{
    None,
    Public,
    Internal,
    Protected,
    Private
}

public record ViewModelCodeFile
{
    public required string FileName { get; init; }
    public required MarkdownDocument CodeMdDocument { get; init; }
    public required string Code { get; init; }
}

//
// public record OperationLog
// {
//     public required string Title { get; init; }
//     public DateTime Timestamp { get; } = DateTime.Now;
//     public required Action Undo { get; init; }
//     public required Action Redo { get; init; }
// }
//
// public partial class OperationLogViewModel : ViewModelBase
// {
//     [BindableDerivedList] private readonly ReadOnlyObservableCollection<OperationLog> _operationLogs;
//     private readonly Stack<OperationLog> _redoes;
//     private readonly SourceCache<OperationLog, DateTime> _undoes;
//     [ObservableAsProperty] private int _count;
//
//     public OperationLogViewModel()
//     {
//         _undoes = new SourceCache<OperationLog, DateTime>(x => x.Timestamp);
//         _undoes.LimitSizeTo(100);
//         _undoes.Connect()
//             .SortAndBind(out _operationLogs, SortExpressionComparer<OperationLog>.Descending(x => x.Timestamp))
//             .Subscribe();
//         _undoes.CountChanged.Do(x => CanUndo = x >= 0).Subscribe();
//         _countHelper = _undoes.CountChanged.ToProperty(this, nameof(Count));
//         _redoes = new Stack<OperationLog>();
//     }
//
//     [Reactive] public partial bool CanUndo { get; private set; }
//     [Reactive] public partial bool CanRedo { get; private set; }
//
//     [ReactiveCommand(CanExecute = nameof(CanUndo))]
//     private void Undo()
//     {
//         var operationLog = OperationLogs[0];
//         operationLog.Undo.Invoke();
//         _undoes.RemoveKey(operationLog.Timestamp);
//         _redoes.Push(operationLog);
//         CanRedo = true;
//     }
//
//     [ReactiveCommand(CanExecute = nameof(CanRedo))]
//     private void Redo()
//     {
//         var operationLog = _redoes.Pop();
//         operationLog.Redo.Invoke();
//         _undoes.AddOrUpdate(operationLog);
//         CanRedo = _redoes.Count > 0;
//     }
//
//     public void AddOperationLog(OperationLog operationLog)
//     {
//         CanRedo = false;
//         _redoes.Clear();
//         _undoes.AddOrUpdate(operationLog);
//     }
//
//     [ReactiveCommand]
//     private void ClearAllOperationLog()
//     {
//         CanRedo = false;
//         _redoes.Clear();
//         _undoes.Clear();
//     }
// }
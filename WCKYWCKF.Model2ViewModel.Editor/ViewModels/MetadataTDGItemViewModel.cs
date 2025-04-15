using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using FuzzySharp;
using Microsoft.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using WCKYWCKF.RxUI.Model2ViewModel.Model;

namespace WCKYWCKF.Model2ViewModel.Editor.ViewModels;

public sealed partial class MetadataTDGItemViewModel : ViewModelBase
{
    private readonly CompositeDisposable _disposables = new();
    [BindableDerivedList] private readonly ReadOnlyObservableCollection<MetadataTDGItemViewModel> _filterMembers;
    private readonly M2VMGenerationInfo _generationInfo;
    internal readonly MainWindowViewModel _mainWindowViewModel;
    [BindableDerivedList] private readonly ReadOnlyObservableCollection<MetadataTDGItemViewModel> _members;
    private readonly SourceCache<M2VMPropertyOrFieldOperationInfo, string> _propertyOrFieldOperationInfos;
    private readonly SourceCache<M2VMReplaceGenerationInfo, string> _replaceGenerationInfos;
    private readonly M2VMTypeInfo? _typeInfo;
    private readonly M2VMTypeMemberInfo? _typeMemberInfo;
    private M2VMPropertyOrFieldOperationInfo? _propertyOrFieldOperationInfo;
    private M2VMReplaceGenerationInfo? _replaceGenerationInfo;

    public MetadataTDGItemViewModel(M2VMTypeInfo typeInfo,
        M2VMGenerationInfo generationInfo,
        SourceCache<M2VMPropertyOrFieldOperationInfo, string> propertyOrFieldOperationInfos,
        SourceCache<M2VMReplaceGenerationInfo, string> replaceGenerationInfos,
        MainWindowViewModel mainWindowViewModel)
    {
        _generationInfo = generationInfo;
        _propertyOrFieldOperationInfos = propertyOrFieldOperationInfos;
        _replaceGenerationInfos = replaceGenerationInfos;
        _mainWindowViewModel = mainWindowViewModel;
        var propertyOrFieldOperationInfosShare = propertyOrFieldOperationInfos.Connect();
        var replaceGenerationInfosShare = replaceGenerationInfos.Connect();

        SourceCache<M2VMTypeMemberInfo, string> members = new(x => x.GetKey());
        var share = members.Connect()
            .LeftJoin(propertyOrFieldOperationInfosShare,
                x => x.GetKey(),
                (_, memberInfo, operationInfo) => (memberInfo, operationInfo))
            .ChangeKey(x => x.memberInfo.GetKey())
            .LeftJoin(replaceGenerationInfos.Connect(),
                x => x.GetKey(),
                (_, tuple, info) => new MetadataTDGItemViewModel(tuple.memberInfo,
                    generationInfo,
                    tuple.operationInfo.HasValue ? tuple.operationInfo.Value : null,
                    info.HasValue ? info.Value : null,
                    propertyOrFieldOperationInfos,
                    replaceGenerationInfos,
                    mainWindowViewModel))
            .SortAndBind(out _members, SortExpressionComparer<MetadataTDGItemViewModel>.Descending(x => x.IsField!)
                .ThenByAscending(x => x.Name))
            .FilterOnObservable(x =>
                _mainWindowViewModel.WhenAnyValue(y => y.Filter_MemberName)
                    .Select(y =>
                        string.IsNullOrWhiteSpace(y) || x.Name.Contains(y, StringComparison.CurrentCultureIgnoreCase)))
            .FilterOnObservable(x =>
                _mainWindowViewModel.WhenAnyValue(y => y.Filter_IsField)
                    .Select(y => y is null || y == x.IsField))
            .FilterOnObservable(x =>
                _mainWindowViewModel.WhenAnyValue(y => y.Filter_IsMemberIncludeInBuild)
                    .Select(y => y is null || y == x.IsMemberIncludedInBuild))
            .FilterOnObservable(x =>
                _mainWindowViewModel.WhenAnyValue(y => y.Filter_MemberCanBeNull)
                    .Select(y => y is null || y == x.IsMemberNullable))
            .FilterOnObservable(x =>
                _mainWindowViewModel.WhenAnyValue(y => y.Filter_MemberAccessibility)
                    .Select(y =>
                        y is FilterAccessibility.None || MainWindowViewModel.GetAccessibilityByFilterAccessibility(y) ==
                        x.Accessibility))
            .FilterOnObservable(x =>
                _mainWindowViewModel.WhenAnyValue(y => y.Filter_MemberGetterAccessibility)
                    .Select(y =>
                        y is FilterAccessibility.None ||
                        MainWindowViewModel.GetAccessibilityByFilterAccessibility(y) == x.GetterAccessibility))
            .FilterOnObservable(x =>
                _mainWindowViewModel.WhenAnyValue(y => y.Filter_MemberSetterAccessibility)
                    .Select(y =>
                        y is FilterAccessibility.None ||
                        MainWindowViewModel.GetAccessibilityByFilterAccessibility(y) == x.SetterAccessibility))
            .FilterOnObservable(x =>
                _mainWindowViewModel.WhenAnyValue(y => y.Filter_MemberGlobeFQType)
                    .Select(y =>
                        string.IsNullOrWhiteSpace(y) ||
                        x.GlobalTypeFullName!.Contains(y, StringComparison.CurrentCultureIgnoreCase)))
            .FilterOnObservable(x =>
                _mainWindowViewModel.WhenAnyValue(y => y.Filter_MemberBuildType)
                    .Select(y =>
                        string.IsNullOrWhiteSpace(y) ||
                        x.MemberBuildType!.Contains(y, StringComparison.CurrentCultureIgnoreCase)))
            .Publish();
        share.SortAndBind(out _filterMembers, SortExpressionComparer<MetadataTDGItemViewModel>
                .Descending(x => x.IsField!)
                .ThenByAscending(x => x.Name))
            .Subscribe();
        share.TrueForAll(x => x.WhenAnyValue(item => item.IsSelected), x => x)
            .Do(x => IsSelected = x)
            .Subscribe();

        propertyOrFieldOperationInfosShare.Subscribe();
        replaceGenerationInfosShare.Subscribe();
        share.Connect();
        members.AddOrUpdate(typeInfo.MemberInfos);
        _typeInfo = typeInfo;
        IsMemberIncludedInBuildByDefault = null;
    }

    public MetadataTDGItemViewModel(M2VMTypeMemberInfo memberInfo,
        M2VMGenerationInfo generationInfo,
        M2VMPropertyOrFieldOperationInfo? propertyOrFieldOperationInfo,
        M2VMReplaceGenerationInfo? replaceGenerationInfo,
        SourceCache<M2VMPropertyOrFieldOperationInfo, string> propertyOrFieldOperationInfos,
        SourceCache<M2VMReplaceGenerationInfo, string> replaceGenerationInfos,
        MainWindowViewModel mainWindowViewModel)
    {
        _typeMemberInfo = memberInfo;
        _members = _filterMembers = new ReadOnlyObservableCollection<MetadataTDGItemViewModel>([]);
        _generationInfo = generationInfo;
        _propertyOrFieldOperationInfo = propertyOrFieldOperationInfo;
        _replaceGenerationInfo = replaceGenerationInfo;
        _propertyOrFieldOperationInfos = propertyOrFieldOperationInfos;
        _replaceGenerationInfos = replaceGenerationInfos;
        _mainWindowViewModel = mainWindowViewModel;
        IsMemberIncludedInBuildByDefault = M2VMHelper.IsDefaultIncluded(_typeMemberInfo!);

        IsMemberIncludedInBuild = !_generationInfo.ShouldIgnoreMember(_typeMemberInfo!);

        IsMemberMarkedNonReplaceable =
            _propertyOrFieldOperationInfo is not null &&
            (_propertyOrFieldOperationInfo.TargetOperation & PropertyOrFieldOperationKind.DoNotReplaceTargetType) > 0;


        IsMemberNullable = _generationInfo.ShouldPropertyTypeBeNullable(_typeMemberInfo!);

        if (IsMemberIncludedInBuild is true)
        {
            MemberBuildTypeLabel =
                _generationInfo.ShouldReplaceMemberType(_typeMemberInfo, out var newGlobalFqType) switch
                {
                    true => "SGVM",
                    false => IsMemberMarkedNonReplaceable is true ? "DoNotReplaceIt" : "default",
                    _ => "replace"
                };
            MemberBuildType = newGlobalFqType;
        }
        else
        {
            MemberBuildTypeLabel = "ignore";
        }

        var first_IsMemberIncludedInBuild = IsMemberIncludedInBuild;
        var first_IsMemberNullable = IsMemberNullable;
        this.WhenAnyValue(x => x.IsMemberIncludedInBuild, x => x.IsMemberNullable)
            .Where(x => x.Item1 != first_IsMemberIncludedInBuild || x.Item2 != first_IsMemberNullable)
            .Do(_ => ConfirmChanges(null))
            .Subscribe();
        MemberCodeText =
            $"{ContainerGlobalTypeFullName} {GetAccessibilityStr(Accessibility!.Value)} {GlobalTypeFullName} {Name} {(IsField is true ? ";" : $"{(GetterAccessibility is null ? "" : GetAccessibilityStr(GetterAccessibility.Value) + " get;")} {(SetterAccessibility is null ? "" : GetAccessibilityStr(SetterAccessibility.Value) + " set;")}")}";

        _mainWindowViewModel.WhenAnyValue(x => x.Filter_MemberCodeText)
            .Throttle(TimeSpan.FromSeconds(0.1))
            .Select(x => Fuzz.WeightedRatio(x ?? string.Empty, MemberCodeText))
            .Do(weightedRatio => WeightedRatioForMemberCodeText = weightedRatio)
            .Subscribe()
            .DisposeWith(_disposables);
        return;

        string? GetAccessibilityStr(Accessibility accessibility)
        {
            return accessibility switch
            {
                Microsoft.CodeAnalysis.Accessibility.Private => "private",
                Microsoft.CodeAnalysis.Accessibility.Protected => "protected",
                Microsoft.CodeAnalysis.Accessibility.Internal => "internal",
                Microsoft.CodeAnalysis.Accessibility.Public => "public",
                _ => null
            };
        }
    }

    public bool? IsMemberIncludedInBuildByDefault { get; }
    public string Name => _typeInfo?.Name ?? _typeMemberInfo!.MemberName;
    public Accessibility? Accessibility => _typeMemberInfo?.Accessibility;
    public Accessibility? GetterAccessibility => _typeMemberInfo?.GetterAccessibility;
    public Accessibility? SetterAccessibility => _typeMemberInfo?.SetterAccessibility;
    public string? GlobalTypeFullName => _typeMemberInfo?.GlobalTypeFullName;
    public bool? IsField => _typeMemberInfo?.IsField;
    public string? ContainerGlobalTypeFullName => _typeMemberInfo?.ContainerGlobalTypeFullName;
    public string? MemberBuildType { get; }
    public string? MemberBuildTypeLabel { get; }
    [Reactive] public partial bool? IsMemberIncludedInBuild { get; set; }
    [Reactive] public bool? IsMemberMarkedNonReplaceable { get; set; }
    [Reactive] public partial bool IsSelected { get; set; }
    [Reactive] public partial bool? IsMemberNullable { get; set; }
    public bool? IsMemberNullableByDefault => !_typeMemberInfo?.IsValue;
    public bool? IsMemberMarkedNonReplaceableByDefault => _typeMemberInfo?.IsMemberTypeSystem;

    public string? MemberCodeText { get; }

    [Reactive] public partial int WeightedRatioForMemberCodeText { get; private set; }

    ~MetadataTDGItemViewModel()
    {
        _disposables.Dispose();
    }

    [ReactiveCommand]
    private void UpdateIsAllSelected()
    {
        if (_typeInfo == null) return;
        var newValue = IsSelected;
        foreach (var metadataTdgItem in Members)
            metadataTdgItem.IsSelected = newValue;
    }

    [ReactiveCommand]
    private void ConfirmChanges(string? newType)
    {
        // var oldValue_propertyOrFieldOperationInfo = _propertyOrFieldOperationInfo;
        // var oldValue_replaceGenerationInfo = _replaceGenerationInfo;
        byte count = 0;
        if (IsMemberIncludedInBuild == IsMemberIncludedInBuildByDefault)
        {
            count += 1;
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.IgnoreProperty, false);
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.IncludePropertyOrField, false);
        }
        else if (IsMemberIncludedInBuild is true && IsMemberIncludedInBuildByDefault is not true)
        {
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.IgnoreProperty, false);
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.IncludePropertyOrField, true);
        }
        else
        {
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.IncludePropertyOrField, false);
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.IgnoreProperty, true);
        }

        if (IsMemberMarkedNonReplaceable == IsMemberMarkedNonReplaceableByDefault)
        {
            count += 1;
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.DoNotReplaceTargetType, false);
        }
        else if (IsMemberMarkedNonReplaceable is false && IsMemberMarkedNonReplaceableByDefault is true)
        {
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.DoNotReplaceTargetType, false);
        }
        else
        {
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.DoNotReplaceTargetType, true);
        }

        if (IsMemberNullable == IsMemberNullableByDefault)
        {
            count += 1;
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.TypeIsNullable, false);
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.TypeIsNotNullable, false);
        }
        else if (IsMemberNullable is true && IsMemberNullableByDefault is not true)
        {
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.TypeIsNotNullable, false);
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.TypeIsNullable, true);
        }
        else
        {
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.TypeIsNullable, false);
            UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind.TypeIsNotNullable, true);
        }

        if ((count == 3 && _propertyOrFieldOperationInfo is not null) ||
            _propertyOrFieldOperationInfo?.TargetOperation is PropertyOrFieldOperationKind.None)
        {
            _propertyOrFieldOperationInfo = null;
            _propertyOrFieldOperationInfos.RemoveKey(_typeMemberInfo!.GetKey());
        }

        if (newType is not null && newType != MemberBuildType)
        {
            if (_replaceGenerationInfo is null)
                _replaceGenerationInfo = new M2VMReplaceGenerationInfo
                {
                    TargetTypeFQType = ContainerGlobalTypeFullName!,
                    TargetMemberName = Name,
                    TargetTypeTypeofInfo = _typeMemberInfo!.MemberTypeInfo,
                    ReplaceFQType = newType,
                    ReplaceTypeTypeofInfo = new TypeofInfo
                    {
                        FQType = null,
                        Sample = M2VMHelper.GetTypeofTargetTypeFQType(newType)
                    }
                };
            else
                _replaceGenerationInfo = _replaceGenerationInfo with
                {
                    ReplaceFQType = newType,
                    ReplaceTypeTypeofInfo = new TypeofInfo
                    {
                        FQType = null,
                        Sample = M2VMHelper.GetTypeofTargetTypeFQType(newType)
                    }
                };
        }
        else if (_replaceGenerationInfo is not null && IsMemberMarkedNonReplaceable is true)
        {
            _replaceGenerationInfo = null;
            _replaceGenerationInfos.RemoveKey(_typeMemberInfo!.GetKey());
        }

        if (_propertyOrFieldOperationInfo is not null)
            _propertyOrFieldOperationInfos.AddOrUpdate(_propertyOrFieldOperationInfo);
        if (_replaceGenerationInfo is not null)
            _replaceGenerationInfos.AddOrUpdate(_replaceGenerationInfo);


        return;

        void UpdatePropertyOrFieldOperationInfo(PropertyOrFieldOperationKind targetOperation, bool add_remove)
        {
            if (_propertyOrFieldOperationInfo is null)
            {
                if (add_remove is false) return;
                _propertyOrFieldOperationInfo = new M2VMPropertyOrFieldOperationInfo
                {
                    TargetTypeFQType = ContainerGlobalTypeFullName!,
                    TargetMemberName = Name,
                    TargetOperation = targetOperation,
                    TargetTypeTypeofInfo = _typeMemberInfo!.MemberTypeInfo
                };
            }
            else
            {
                _propertyOrFieldOperationInfo = _propertyOrFieldOperationInfo with
                {
                    TargetOperation = add_remove switch
                    {
                        true => AddFlag(_propertyOrFieldOperationInfo.TargetOperation, targetOperation),
                        _ => RemoveFlag(_propertyOrFieldOperationInfo.TargetOperation, targetOperation)
                    }
                };
            }
        }

        PropertyOrFieldOperationKind RemoveFlag(PropertyOrFieldOperationKind target,
            PropertyOrFieldOperationKind operationKind)
        {
            if (target.HasFlag(operationKind))
                return target ^ operationKind;
            return target;
        }

        PropertyOrFieldOperationKind AddFlag(PropertyOrFieldOperationKind target,
            PropertyOrFieldOperationKind operationKind)
        {
            if (!target.HasFlag(operationKind))
                return target | operationKind;
            return target;
        }
    }

    public string? GetKey()
    {
        return _typeMemberInfo?.GetKey();
    }

    [ReactiveCommand]
    private void OpenEditMemberBuildTypeDialog()
    {
        MessageBus.Current.SendMessage(new OpenEditMemberBuildTypeDialogEvent(this));
    }
    // [ReactiveCommand]
    // private void Restore()
    // {
    //     if (_typeMemberInfo is null) return;
    //
    //     if (_propertyOrFieldOperationInfos.Count > 0)
    //         _propertyOrFieldOperationInfos.Clear();
    //     if (_replaceGenerationInfos.Count > 0)
    //         _replaceGenerationInfos.Clear();
    // }
}
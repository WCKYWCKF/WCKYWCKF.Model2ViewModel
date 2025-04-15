using System;
using Irihi.Avalonia.Shared.Contracts;
using Markdig;
using Markdig.Syntax;
using ReactiveUI.SourceGenerators;

namespace WCKYWCKF.Model2ViewModel.Editor.ViewModels;

public partial class EditMemberBuildTypeDialogViewModel : ViewModelBase, IDialogContext
{
    public const string EditMemberBuildTypeDialogMdText =
        """
        用于编辑成员在生成中的类型。

        * 本会话正在编辑的成员是`{0}.{1}`

        * {2}

        ::: warning
        当成员没有被纳入生成中时，所有设置不可用并且会被忽略。
        :::

        ---

        @@@

        ::: info
        当成员被纳入生成中但你不想替换成员的类型时，可以启用此设置。  
        启用后，主动设置的替换类型或默认替换类型（SGVM）将被忽略，成员的类型将保持原样。
        :::

        ::: warning
        当成员类型在生成中默认不会被替换时，此设置不可用并且会被忽略。
        :::

        ---

        成员原本的类型是`{3}`  

        成员在生成中的类型是`{4}`

        @@@

        ::: info
        当成员被纳入生成中且你希望替换成员的类型时，可以使用此设置。
        :::

        ::: warning
        当启用“不要替换成员类型（主动设置）”时，此设置不可用并且会被忽略。
        :::
        """;

    private readonly MetadataTDGItemViewModel _metadataTdgItemViewModel;

    public EditMemberBuildTypeDialogViewModel(MetadataTDGItemViewModel metadataTdgItemViewModel)
    {
        _metadataTdgItemViewModel = metadataTdgItemViewModel;
        EditMemberBuildTypeDialogMdDocument = Markdown.Parse(
            string.Format(EditMemberBuildTypeDialogMdText,
                metadataTdgItemViewModel.ContainerGlobalTypeFullName,
                metadataTdgItemViewModel.Name,
                metadataTdgItemViewModel.IsMemberIncludedInBuild is true
                    ? "此成员已被纳入生成中"
                    : "此成员没有被纳入生成中",
                metadataTdgItemViewModel.GlobalTypeFullName,
                metadataTdgItemViewModel.MemberBuildType ?? "此成员没有被纳入生成中"),
            MainWindowViewModel.MDPipeline);
        IsMemberMarkedNonReplaceable = metadataTdgItemViewModel.IsMemberMarkedNonReplaceable is true;
        NewTypeName = metadataTdgItemViewModel.MemberBuildTypeLabel is "replace"
            ? metadataTdgItemViewModel.MemberBuildType
            : null;
    }

    [Reactive] public partial bool IsMemberMarkedNonReplaceable { get; set; }
    [Reactive] public partial string? NewTypeName { get; set; }

    public bool CanSetIsMemberMarkedNonReplaceable =>
        _metadataTdgItemViewModel.IsMemberMarkedNonReplaceableByDefault is false
        || _metadataTdgItemViewModel.MemberBuildTypeLabel is "replace";

    public bool IsUsable => _metadataTdgItemViewModel.IsMemberIncludedInBuild is true;

    public MarkdownDocument EditMemberBuildTypeDialogMdDocument { get; }


    [ReactiveCommand]
    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;

    [ReactiveCommand]
    private void Confirm()
    {
        Close();
        if (IsUsable is false) return;
        _metadataTdgItemViewModel.IsMemberMarkedNonReplaceable = IsMemberMarkedNonReplaceable;
        _metadataTdgItemViewModel.ConfirmChangesCommand.Execute(NewTypeName!).Subscribe();
    }

    // [ReactiveCommand]
    // private void ConfirmAll(string newType)
    // {
    //     Confirm();
    //     var task = _metadataTdgItemViewModel.ConfirmChangesCommand.Execute();
    //     var operationLog = new OperationLog
    //     {
    //         Title = $"修改了：{_metadataTdgItemViewModel.ContainerGlobalTypeFullName}.{_metadataTdgItemViewModel.Name}的生成设置",
    //         Undo = () =>
    //         {
    //             _oldValue.SetTarget(_metadataTdgItemViewModel);
    //             _metadataTdgItemViewModel.ConfirmChangesCommand.Execute();
    //             Notification notification = new()
    //             {
    //                 Title = "对成员构建设置的修改已回退",
    //                 Message =
    //                     $"成员：{_metadataTdgItemViewModel.ContainerGlobalTypeFullName}.{_metadataTdgItemViewModel.Name}已恢复至{DateTime.Now.ToLongTimeString()}时的状态。",
    //                 Type = NotificationType.Success
    //             };
    //             MessageBus.Current.SendMessage(notification);
    //         },
    //         Redo = () =>
    //         {
    //             task.GetAwaiter().GetResult().SetTarget(_metadataTdgItemViewModel);
    //             _metadataTdgItemViewModel.ConfirmChangesCommand.Execute();
    //             Notification notification = new()
    //             {
    //                 Title = "对成员构建设置的修改已重做",
    //                 Message =
    //                     $"成员：{_metadataTdgItemViewModel.ContainerGlobalTypeFullName}.{_metadataTdgItemViewModel.Name}已恢复至{DateTime.Now.ToLongTimeString()}时的状态。",
    //                 Type = NotificationType.Success
    //             };
    //             MessageBus.Current.SendMessage(notification);
    //         }
    //     };
    //     Notification notification = new()
    //     {
    //         Title = "对成员构建设置的修改已生效",
    //         Message = $"修改的成员：{_metadataTdgItemViewModel.ContainerGlobalTypeFullName}.{_metadataTdgItemViewModel.Name}",
    //         Type = NotificationType.Success
    //     };
    //     MessageBus.Current.SendMessage(notification);
    //     _metadataTdgItemViewModel._mainWindowViewModel.OperationLogViewModel.AddOperationLog(operationLog);
    // }
}
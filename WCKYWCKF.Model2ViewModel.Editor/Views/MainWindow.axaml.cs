using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Microsoft.CodeAnalysis;
using ReactiveUI;
using Ursa.Controls;
using Ursa.ReactiveUIExtension;
using WCKYWCKF.Model2ViewModel.Editor.ViewModels;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace WCKYWCKF.Model2ViewModel.Editor.Views;

public partial class MainWindow : ReactiveUrsaWindow<MainWindowViewModel>
{
    private ToggleButton? _lates_TabItem;
    private WindowNotificationManager? _notificationManager;

    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(WhenActivated);
        // ((TabStrip)RightToolsTabStrip).Events().PropertyChanged
        //     .Where(x => x.Property == TabStrip.SelectedIndexProperty)
        //     .Do(UpdateRightToolsTransitioningContentControlAnimationDirection)
        //     .Subscribe();
        // LogicalChildren.AddRange(RightToolsTabStrip.Items
        //     .OfType<TabStripItem>()
        //     .Select(x => x.Tag)
        //     .OfType<Control>());
    }

    public static FuncValueConverter<Accessibility?, string?> AccessibilityToStringConverter { get; } =
        new(accessibility => accessibility switch
        {
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.Public => "public",
            _ => null
        });

    public static FuncValueConverter<bool?, string?> IsMemberSettingByDefaultToStringConverter { get; } =
        new(isMemberIncludedInBuildByDefault => isMemberIncludedInBuildByDefault switch
        {
            true => "default",
            _ => null
        });

    private void UpdateRightToolsTransitioningContentControlAnimationDirection(AvaloniaPropertyChangedEventArgs obj)
    {
        // RightToolsTransitioningContentControl.IsTransitionReversed = (int)obj.OldValue! > (int)obj.NewValue!;
    }

    private void WhenActivated(CompositeDisposable disposable)
    {
        MessageBus.Current.Listen<Notification>().Do(ShowNotification).Subscribe().DisposeWith(disposable);
        MessageBus.Current.Listen<OpenEditMemberBuildTypeDialogEvent>().Do(OpenEditMemberBuildTypeDialog).Subscribe()
            .DisposeWith(disposable);
    }

    private void ShowNotification(Notification notification)
    {
        _notificationManager?.Show(
            notification,
            showIcon: true,
            showClose: true,
            type: notification.Type);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var topLevel = GetTopLevel(this);
        _notificationManager = new WindowNotificationManager(topLevel)
        {
            MaxItems = 4,
            Position = NotificationPosition.BottomRight
        };
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _notificationManager?.Uninstall();
    }

    private void OpenEditMemberBuildTypeDialog(OpenEditMemberBuildTypeDialogEvent e)
    {
        var _overlayDialogOptions = new OverlayDialogOptions
        {
            Title = "编辑成员构建类型",
            CanResize = false,
            HorizontalAnchor = HorizontalPosition.Right,
            IsCloseButtonVisible = false,
            Buttons = DialogButton.None
        };
        OverlayDialog.Show<EditMemberBuildTypeDialog, EditMemberBuildTypeDialogViewModel>(
            new EditMemberBuildTypeDialogViewModel(e.Source),
            options: _overlayDialogOptions);
        // if (sender is Control { Tag: MetadataTDGItemViewModel tag })
        // {
        //     _overlayDialogOptions ??= new OverlayDialogOptions
        //     {
        //         Title = "编辑成员构建类型",
        //         CanResize = false,
        //         HorizontalAnchor = HorizontalPosition.Right,
        //         IsCloseButtonVisible = false,
        //         Buttons = DialogButton.None
        //     };
        //     OverlayDialog.Show<EditMemberBuildTypeDialog, EditMemberBuildTypeDialogViewModel>(
        //         new EditMemberBuildTypeDialogViewModel(tag),
        //         options: _overlayDialogOptions);
        // }
        // else
        // {
        //     Notification notification = new()
        //     {
        //         Title = "没能成功打开编辑成员类型对话框",
        //         Content = "请检查是否有成员被选中",
        //         Type = NotificationType.Error
        //     };
        //     MessageBus.Current.SendMessage(notification);
        // }
    }

    private void InputElement_OnPointerReleased(object? sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is not ToggleButton tabItem) return;
        if ((tabItem == _lates_TabItem || _lates_TabItem is null) && tabItem.IsChecked is false)
        {
            if (_lates_TabItem is not null)
                _lates_TabItem.IsChecked = false;
            Tools_ContentControl.Content = null;
            _lates_TabItem = null;
        }
        else
        {
            if (_lates_TabItem is not null)
                _lates_TabItem.IsChecked = false;
            _lates_TabItem = tabItem;
            Tools_ContentControl.Content = tabItem.Tag;
        }
    }

    private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == SplitView.IsPaneOpenProperty && e.NewValue is false && _lates_TabItem is not null)
        {
            _lates_TabItem.IsChecked = false;
            Tools_ContentControl.Content = null;
            _lates_TabItem = null;
        }
    }

    private async void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (ViewModel is null) return;
            await ViewModel.PreviewGeneratedCodeCommand.Execute();
            var _overlayDialogOptions = new OverlayDialogOptions
            {
                Title = "正在预览将会构建的代码",
                CanResize = false,
                FullScreen = true,
                IsCloseButtonVisible = true,
                Buttons = DialogButton.None
            };
            await OverlayDialog.ShowModal<PreviewGeneratedCodeDialog, MainWindowViewModel>(
                ViewModel!,
                options: _overlayDialogOptions);
        }
        catch (Exception exception)
        {
            Notification notification = new()
            {
                Title = "预览生成代码失败",
                Content = exception.Message,
                Type = NotificationType.Error
            };
            MessageBus.Current.SendMessage(notification);
        }
    }
}
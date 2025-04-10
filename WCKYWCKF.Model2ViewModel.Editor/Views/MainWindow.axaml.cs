using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Media;
using WCKYWCKF.Model2ViewModel.Editor.ViewModels;

namespace WCKYWCKF.Model2ViewModel.Editor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TagProperty)
        {
            var tag = e.NewValue;
        }
    }

    private BindingExpressionBase? test;

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        (sender as ListBox)!.DataContext = DataContext;
        test = (sender as ListBox)?.Bind(ListBox.SelectedIndexProperty, new Binding()
        {
            Source = DataContext,
            Path = nameof(MainWindowViewModel.SelectedIndex),
            Mode = BindingMode.TwoWay
        });
    }

    private void Control_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        test?.Dispose();
    }
}
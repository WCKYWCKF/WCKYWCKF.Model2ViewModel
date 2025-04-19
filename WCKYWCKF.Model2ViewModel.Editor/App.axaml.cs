using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Irihi.Mantra.Markdown;
using Irihi.Mantra.Markdown.BlockGenerators;
using Irihi.Mantra.Markdown.Plugin.AvaloniaHybrid;
using WCKYWCKF.Model2ViewModel.Editor.ViewModels;
using WCKYWCKF.Model2ViewModel.Editor.Views;

namespace WCKYWCKF.Model2ViewModel.Editor;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        MarkdownBlockGenerators.Default.Plugins.Add(new FencedCodeBlockGenerator());
        MarkdownBlockGenerators.Default.Plugins.Add(new AvaloniaHybridBlockGenerator());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

        base.OnFrameworkInitializationCompleted();
    }
}
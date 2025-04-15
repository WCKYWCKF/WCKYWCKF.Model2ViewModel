using Avalonia.Controls;
using Avalonia.Data.Converters;
using Irihi.Mantra.Markdown;

namespace WCKYWCKF.Model2ViewModel.Editor.Views;

public partial class PreviewGeneratedCodeDialog : UserControl
{
    public PreviewGeneratedCodeDialog()
    {
        InitializeComponent();
        MarkdownView markdownView = default;
        // markdownView.FindControl<ScrollViewer>()
    }

    public static FuncValueConverter<double, double> DoubleToDoubleConverter { get; } = new(value => value - 100);
}
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace WCKYWCKF.Model2ViewModel.Tests.Utils;

public class TestAdditionalFile : AdditionalText
{
    private readonly SourceText _text;

    public TestAdditionalFile(string path, string text)
    {
        Path = path;
        _text = SourceText.From(text);
    }

    public override string Path { get; }

    public override SourceText GetText(CancellationToken cancellationToken = new())
    {
        return _text;
    }
}
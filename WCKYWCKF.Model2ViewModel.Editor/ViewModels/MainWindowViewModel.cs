using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Extensions;

namespace WCKYWCKF.Model2ViewModel.Editor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [Reactive] public partial SourceList<string> Source { get; set; }
    [Reactive] public partial ReadOnlyObservableCollection<string> SourceReadOnly { get; set; }
    [Reactive] public partial int SelectedIndex { get; set; }
    [Reactive] public partial bool VTest { get; set; }

    public MainWindowViewModel()
    {
        Source = new SourceList<string>();
        _source.Connect()
            .Bind(out _sourceReadOnly)
            .Subscribe();
        SelectedIndex = 2;
        this.WhenAnyValue(x => x.SelectedIndex)
            .Do(OnNext)
            .Subscribe();
    }

    private void OnNext(int obj)
    {
    }

    [ReactiveCommand]
    public void Ref()
    {
        VTest = !VTest;
        return;
        Source.Clear();
        Source.Add(Path.GetRandomFileName());
        Source.Add(Path.GetRandomFileName());
        Source.Add(Path.GetRandomFileName());
        Source.Add(Path.GetRandomFileName());
        Source.Add(Path.GetRandomFileName());
        Source.Add(Path.GetRandomFileName());
    }
}
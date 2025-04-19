using System;

namespace WCKYWCKF.Model2ViewModel.Editor.ViewModels;

public class OpenEditMemberBuildTypeDialogEvent(MetadataTDGItemViewModel source) : EventArgs
{
    public MetadataTDGItemViewModel Source { get; } = source;
}
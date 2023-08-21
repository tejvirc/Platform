namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor;
using static Store.IdleText.IdleTextSelectors;

public class IdleTextViewModel : ObservableObject, IActivatableViewModel
{
    private string? _idleText;

    public IdleTextViewModel(IStore store)
    {
        this.WhenActivated(disposables =>
        {
            store
                .Select(IdleTextSelector)
                .Subscribe(text =>
                {
                    IdleText = text;
                })
                .DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();

    public string? IdleText
    {
        get => _idleText;

        set => SetProperty(ref _idleText, value);
    }
}

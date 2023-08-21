namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using Commands;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Extensions.Fluxor;
using Fluxor;
using static Store.Upi.UpiSelectors;

public class StandardUpiViewModel : ObservableObject, IActivatableViewModel
{
    private readonly IDispatcher _dispatcher;

    private bool _cashOutEnabled;
    private bool _volumeButtonVisible;
    private bool _serviceButtonVisible;
    private bool _vbdServiceButtonDisabled;

    public StandardUpiViewModel(IDispatcher dispatcher, IStore store, IApplicationCommands commands)
    {
        _dispatcher = dispatcher;

        Volume = new VolumeViewModel(dispatcher, store, commands);

        CashOutEnabled = true;

        ShutdownCommand = new RelayCommand(OnShutdown);
        CashOutCommand = new RelayCommand(OnCashOut);

        commands.ShutdownCommand.RegisterCommand(ShutdownCommand);

        this.WhenActivated(disposables =>
        {
            store
                .Select(SelectServiceAvailable)
                .Subscribe(OnServiceAvailableChanged)
                .DisposeWith(disposables);

            store
                .Select(SelectServiceAvailable)
                .Subscribe(OnServiceEnabledChanged)
                .DisposeWith(disposables);

            store
                .Select(SelectVolumeControlEnabled)
                .Subscribe(OnVolumeControlEnabledChanged)
                .DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();

    public RelayCommand ShutdownCommand { get; }

    public RelayCommand CashOutCommand { get; }

    public VolumeViewModel Volume { get; }

    public bool CashOutEnabled
    {
        get => _cashOutEnabled;

        set => SetProperty(ref _cashOutEnabled, value);
    }

    public bool ServiceButtonVisible
    {
        get => _serviceButtonVisible;

        set => SetProperty(ref _serviceButtonVisible, value);
    }

    public bool VbdServiceButtonDisabled
    {
        get => _vbdServiceButtonDisabled;

        set => SetProperty(ref _vbdServiceButtonDisabled, value);
    }

    public bool VolumeButtonVisible
    {
        get => _volumeButtonVisible;

        set => SetProperty(ref _volumeButtonVisible, value);
    }

    private void OnShutdown()
    {
    }

    private void OnCashOut()
    {
    }

    private void OnServiceAvailableChanged(bool isServiceAvailable)
    {
        ServiceButtonVisible = isServiceAvailable;
    }

    private void OnServiceEnabledChanged(bool isServiceEnabled)
    {
        VbdServiceButtonDisabled = !isServiceEnabled;
    }

    private void OnVolumeControlEnabledChanged(bool isVolumeControlEnabled)
    {
        VolumeButtonVisible = isVolumeControlEnabled;
    }
}

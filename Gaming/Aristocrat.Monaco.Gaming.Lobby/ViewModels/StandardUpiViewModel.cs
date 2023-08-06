namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using Commands;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Extensions.Fluxor;
using Fluxor;
using static Store.Upi.UpiSelectors;

public class StandardUpiViewModel : ObservableObject
{
    private readonly IDispatcher _dispatcher;

    private readonly SubscriptionList _subscriptions = new();

    private bool _cashOutEnabled;
    private bool _volumeButtonVisible;
    private bool _serviceButtonVisible;
    private bool _vbdServiceButtonDisabled;

    public StandardUpiViewModel(IDispatcher dispatcher, IStoreSelector selector, IApplicationCommands commands)
    {
        _dispatcher = dispatcher;

        Volume = new VolumeViewModel(dispatcher, selector, commands);

        CashOutEnabled = true;

        ShutdownCommand = new RelayCommand(OnShutdown);
        CashOutCommand = new RelayCommand(OnCashOut);

        commands.ShutdownCommand.RegisterCommand(ShutdownCommand);

        _subscriptions += selector.Select(SelectServiceAvailable).Subscribe(OnServiceAvailableChanged);
        _subscriptions += selector.Select(SelectServiceAvailable).Subscribe(OnServiceEnabledChanged);
        _subscriptions += selector.Select(SelectVolumeControlEnabled).Subscribe(OnVolumeControlEnabledChanged);
    }

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
        _subscriptions.UnsubscribeAll();
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

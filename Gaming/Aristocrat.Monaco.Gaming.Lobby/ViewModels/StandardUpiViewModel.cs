namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using Commands;
using Common;
using Extensions.Fluxor;
using Fluxor;
using Prism.Commands;
using Prism.Mvvm;
using static Store.Upi.UpiSelectors;

public class StandardUpiViewModel : BindableBase
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

        ShutdownCommand = new DelegateCommand(OnShutdown);
        CashOutCommand = new DelegateCommand(OnCashOut);

        commands.ShutdownCommand.RegisterCommand(ShutdownCommand);

        _subscriptions += selector.Select(SelectServiceAvailable).Subscribe(OnServiceAvailableChanged);
        _subscriptions += selector.Select(SelectServiceAvailable).Subscribe(OnServiceEnabledChanged);
        _subscriptions += selector.Select(SelectVolumeControlEnabled).Subscribe(OnVolumeControlEnabledChanged);
    }

    public DelegateCommand ShutdownCommand { get; }

    public DelegateCommand CashOutCommand { get; }

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

namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using System.Collections.Generic;
using Commands;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Extensions.Fluxor;
using Fluxor;
using Hardware.Contracts.Audio;
using Store;
using static Store.Audio.AudioSelectors;

public class VolumeViewModel : ObservableObject
{
    private const string Volume0Key = "Volume0Normal";
    private const string Volume1Key = "Volume1Normal";
    private const string Volume2Key = "Volume2Normal";
    private const string Volume3Key = "Volume3Normal";
    private const string Volume4Key = "Volume4Normal";
    private const string Volume5Key = "Volume5Normal";

    private readonly IDispatcher _dispatcher;

    private readonly SubscriptionList _subscriptions = new();

    private int _volumeValue;

    public VolumeViewModel(IDispatcher dispatcher, IStoreSelector selector, IApplicationCommands commands)
    {
        _dispatcher = dispatcher;

        ShutdownCommand = new RelayCommand(OnShutdown);
        VolumeCommand = new RelayCommand(OnVolume);

        commands.ShutdownCommand.RegisterCommand(ShutdownCommand);

        _subscriptions += selector.Select(SelectPlayerVolumeScalar).Subscribe(OnPlayerVolumeScalarChanged);
    }

    public RelayCommand ShutdownCommand { get; }

    public RelayCommand VolumeCommand { get; }

    public List<string> ResourceKeys { get; } = new List<string>
        {
            Volume0Key,
            Volume1Key,
            Volume2Key,
            Volume3Key,
            Volume4Key,
            Volume5Key
        };

    public int VolumeValue
    {
        get => _volumeValue;

        set => SetProperty(ref _volumeValue, value);
    }

    private void OnShutdown()
    {
        _subscriptions.UnsubscribeAll();
    }

    private void OnVolume() => _dispatcher.Dispatch(new ChangeVolumeAction());

    private void OnPlayerVolumeScalarChanged(VolumeScalar volumeScaler)
    {
        VolumeValue = (int)volumeScaler;
    }
}

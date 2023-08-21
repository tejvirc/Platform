namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using System.Collections.Generic;
using Commands;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fluxor;
using Hardware.Contracts.Audio;
using Store;
using static Store.Audio.AudioSelectors;

public class VolumeViewModel : ObservableObject, IActivatableViewModel
{
    private const string Volume0Key = "Volume0Normal";
    private const string Volume1Key = "Volume1Normal";
    private const string Volume2Key = "Volume2Normal";
    private const string Volume3Key = "Volume3Normal";
    private const string Volume4Key = "Volume4Normal";
    private const string Volume5Key = "Volume5Normal";

    private readonly IDispatcher _dispatcher;

    private int _volumeValue;

    public VolumeViewModel(IDispatcher dispatcher, IStore store, IApplicationCommands commands)
    {
        _dispatcher = dispatcher;

        ShutdownCommand = new RelayCommand(OnShutdown);
        VolumeCommand = new RelayCommand(OnVolume);

        commands.ShutdownCommand.RegisterCommand(ShutdownCommand);

        this.WhenActivated(disposables =>
        {
            store
                .Select(SelectPlayerVolumeScalar)
                .Subscribe(OnPlayerVolumeScalarChanged)
                .DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();

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
    }

    private void OnVolume() => _dispatcher.Dispatch(new AudioChangeVolumeAction());

    private void OnPlayerVolumeScalarChanged(VolumeScalar volumeScaler)
    {
        VolumeValue = (int)volumeScaler;
    }
}

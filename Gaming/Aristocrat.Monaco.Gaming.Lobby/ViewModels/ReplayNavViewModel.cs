namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using Extensions.Fluxor;
using Localization.Properties;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Prism.Commands;
using Prism.Mvvm;
using static Store.Replay.ReplaySelectors;

public class ReplayNavViewModel : BindableBase
{
    private static readonly string CompletionTextKey = ResourceKeys.ReplayCompletedText;
    private const double CashOutMessagesCycleIntervalInSeconds = 3.0;

    private bool _isReplayNavigationVisible;
    private double _backgroundOpacity;
    private string? _messageText;
    private long? _replaySequence;
    private string? _replayGameName;
    private DateTime? _replayStartTime;
    private string? _label;
    private string? _cashoutText;
    private string? _replayPauseMessageText;

    public ReplayNavViewModel(IStoreSelector selector)
    {
        selector.Select(SelectReplayGameName).Subscribe(x => ReplayGameName = x);
        selector.Select(SelectReplayGameName).Subscribe(x => ReplayGameName = x);
        selector.Select(SelectReplayLabel).Subscribe(x => Label = x);
        selector.Select(SelectReplaySequence).Subscribe(x => ReplaySequence = x);
        selector.Select(SelectReplayStartTime).Subscribe(x => ReplayStartTime = x);
        // selector.Select(SelectReplayEndCredits).Subscribe(x => ReplayEndCredits = x);
        selector.Select(SelectReplayGameWinBonusAwarded).Subscribe(x => GameWinBonusAwarded = x);
        selector.Select(SelectReplayVoucherOut).Subscribe(x => VoucherOut = x);
        selector.Select(SelectReplayHardMeterOut).Subscribe(x => HardMeterOut = x);
        selector.Select(SelectReplayBonusOrGameWinToCredits).Subscribe(x => HardMeterOut = x);
        selector.Select(SelectReplayBonusOrGameWinToHandpay).Subscribe(x => HardMeterOut = x);
        selector.Select(SelectReplayCancelledCredits).Subscribe(x => HardMeterOut = x);
    }

    public DelegateCommand ExitCommand { get; }

    public DelegateCommand ContinueCommand { get; }

    public bool IsReplayNavigationVisible
    {
        get => _isReplayNavigationVisible;

        set => SetProperty(ref _isReplayNavigationVisible, value);
    }

    public double BackgroundOpacity
    {
        get => _backgroundOpacity;

        set
        {
            if (Math.Abs(_backgroundOpacity - value) < 0.001)
            {
                return;
            }

            _backgroundOpacity = value;
            RaisePropertyChanged(nameof(BackgroundOpacity));
        }
    }

    public string? MessageText
    {
        get => _messageText;

        set => SetProperty(ref _messageText, value);
    }

    public long? ReplaySequence
    {
        get => _replaySequence;

        set => SetProperty(ref _replaySequence, value);
    }

    public string? ReplayGameName
    {
        get => _replayGameName;

        set => SetProperty(ref _replayGameName, value);
    }

    public DateTime? ReplayStartTime
    {
        get => _replayStartTime;

        set => SetProperty(ref _replayStartTime, value);
    }

    public string? Label
    {
        get => _label;

        set => SetProperty(ref _label, value);
    }

    public string? CashoutText
    {
        get => _cashoutText;

        set => SetProperty(ref _cashoutText, value);
    }

    public string? ReplayPauseMessageText
    {
        get => _replayPauseMessageText;

        set
        {
            if (_replayPauseMessageText == value)
            {
                return;
            }

            _replayPauseMessageText = value;
            RaisePropertyChanged(nameof(ReplayPauseMessageText));
            RaisePropertyChanged(nameof(IsReplayPauseMessageVisible));
            RaisePropertyChanged(nameof(CanReplayContinue));
        }
    }

    public bool IsReplayPauseMessageVisible => !string.IsNullOrEmpty(_replayPauseMessageText);

    public bool CanReplayContinue =>
        !string.IsNullOrEmpty(_replayPauseMessageText) && _replayPauseMessageText != CompletionText;
}

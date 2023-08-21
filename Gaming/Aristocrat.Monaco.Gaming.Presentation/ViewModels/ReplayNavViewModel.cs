namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Timers;
using Application.Contracts.Extensions;
using Application.Contracts.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Extensions.Fluxor;
using Fluxor;
using Localization.Properties;
using Store;
using Store.Replay;
using static Store.Replay.ReplaySelectors;

public class ReplayNavViewModel : ObservableObject, IActivatableViewModel
{
    private const double CashOutMessagesCycleIntervalInSeconds = 3.0;

    private readonly IState<ReplayState> _replayState;
    private readonly IDispatcher _dispatcher;

    private readonly List<string> _cashOutTexts = new();
    private readonly Timer _cashOutMessageTimer;

    private bool _isReplayNavigationVisible;
    private double _backgroundOpacity;
    private string? _messageText;
    private long? _replaySequence;
    private string? _replayGameName;
    private DateTime? _replayStartTime;
    private string? _label;
    private string? _cashOutText;
    private string? _replayPauseMessageKey;
    private bool _isReplayPauseMessageVisible;
    private bool _canReplayContinue;
    private int _cashOutMessageIndex;

    public ReplayNavViewModel(IStore store, IState<ReplayState> replayState, IDispatcher dispatcher)
    {
        _replayState = replayState;
        _dispatcher = dispatcher;
        _cashOutMessageTimer = new Timer(TimeSpan.FromSeconds(CashOutMessagesCycleIntervalInSeconds).TotalMilliseconds);
        _cashOutMessageTimer.Elapsed += CashOutMessageTimerTick;

        ExitCommand = new RelayCommand(OnExit);
        ContinueCommand = new RelayCommand(OnContinue);

        this.WhenActivated(disposables =>
        {
            store
                .Select(SelectReplayStarted)
                .WhenTrue()
                .Subscribe(_ => OnStarted())
                .DisposeWith(disposables);

            store
                .Select(SelectReplayPaused)
                .WhenTrue()
                .Subscribe(_ => OnPaused())
                .DisposeWith(disposables);

            store
                .Select(SelectReplayCompleted)
                .WhenTrue()
                .Subscribe(_ => OnCompleted())
                .DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();

    public RelayCommand ExitCommand { get; }

    public RelayCommand ContinueCommand { get; }

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
            OnPropertyChanged(nameof(BackgroundOpacity));
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

    public string? CashOutText
    {
        get => _cashOutText;

        set => SetProperty(ref _cashOutText, value);
    }

    public string? ReplayPauseMessageKey
    {
        get => _replayPauseMessageKey;

        set => SetProperty(ref _replayPauseMessageKey, value);
    }

    public bool IsReplayPauseMessageVisible
    {
        get => _isReplayPauseMessageVisible;

        set => SetProperty(ref _isReplayPauseMessageVisible, value);
    }

    public bool CanReplayContinue
    {
        get => _canReplayContinue;

        set => SetProperty(ref _canReplayContinue, value);
    }

    private void CashOutMessageTimerTick(object? source, ElapsedEventArgs e)
    {
        _cashOutMessageIndex = (_cashOutMessageIndex + 1) % _cashOutTexts.Count;
        CashOutText = _cashOutTexts.ElementAt(_cashOutMessageIndex);
    }

    private void OnStarted()
    {
        IsReplayPauseMessageVisible = false;
        CanReplayContinue = false;

        ReplayPauseMessageKey = string.Empty;

        ReplayGameName = _replayState.Value.GameName;
        Label = _replayState.Value.Label;
        ReplaySequence = _replayState.Value.Sequence;
        ReplayStartTime = _replayState.Value.StartTime;

        AppendGameWinBonusText();
        AppendVoucherInfoText();
        AppendHardMeterOutText();
        AppendHandpayInfo();
    }

    private void AppendGameWinBonusText()
    {
        if (_replayState.Value.GameWinBonusAwarded > 0)
        {
            _cashOutTexts.Add(Localizer.For(CultureFor.Player).FormatString(
                ResourceKeys.ReplayGameWinBonusAwarded, _replayState.Value.GameWinBonusAwarded
                    .FormattedCurrencyString()));
        }
    }

    private void AppendVoucherInfoText()
    {
        if (_replayState.Value.HardMeterOut > 0)
        {
            _cashOutTexts.Add(Localizer.For(CultureFor.Player)
                .FormatString(ResourceKeys.ReplayTicketPrinted) + " " + _replayState.Value.VoucherOut.FormattedCurrencyString());
        }
    }

    private void AppendHardMeterOutText()
    {
        if (_replayState.Value.HardMeterOut > 0)
        {
            _cashOutTexts.Add(Localizer.For(CultureFor.Player)
                .FormatString(ResourceKeys.ReplayTicketPrinted) + " " + _replayState.Value.HardMeterOut.FormattedCurrencyString());
        }
    }

    private void AppendHandpayInfo()
    {
        if (_replayState.Value.BonusOrGameWinToCredits > 0)
        {
            _cashOutTexts.Add(Localizer.For(CultureFor.Operator).FormatString(
                ResourceKeys.JackpotToCreditsKeyedOff, _replayState.Value.BonusOrGameWinToCredits
                    .FormattedCurrencyString()));
        }

        if (_replayState.Value.BonusOrGameWinToHandpay > 0)
        {
            _cashOutTexts.Add(Localizer.For(CultureFor.Operator).FormatString(
                ResourceKeys.JackpotHandpayKeyedOff, _replayState.Value.BonusOrGameWinToHandpay
                    .FormattedCurrencyString()));
        }

        if (_replayState.Value.CancelledCredits > 0)
        {
            _cashOutTexts.Add(Localizer.For(CultureFor.Operator).FormatString(
                ResourceKeys.CashOutHandpayKeyedOff, _replayState.Value.CancelledCredits
                    .FormattedCurrencyString()));
        }
    }

    private void OnPaused()
    {
        IsReplayPauseMessageVisible = false;
        CanReplayContinue = false;

        ReplayPauseMessageKey = ResourceKeys.ReplayPauseInputText;
    }

    private void OnCompleted()
    {
        IsReplayPauseMessageVisible = false;
        CanReplayContinue = false;

        ReplaySequence = null;
        ReplayGameName = string.Empty;
        ReplayStartTime = null;
        Label = string.Empty;
        CashOutText = string.Empty;
        MessageText = string.Empty;

        ReplayPauseMessageKey = string.Empty;
        ReplayPauseMessageKey = ResourceKeys.ReplayCompletedText;
    }

    private void OnExit()
    {
        _dispatcher.Dispatch(new ReplayExitAction());
    }

    private void OnContinue()
    {
        ReplayPauseMessageKey = string.Empty;

        _dispatcher.Dispatch(new ReplayContinueAction());
    }
}

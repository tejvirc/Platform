namespace Aristocrat.Monaco.Gaming.Lobby.Services.Replay;

using System;
using System.Linq;
using Accounting.Contracts;
using Accounting.Contracts.HandCount;
using Accounting.Contracts.Handpay;
using Application.Contracts.Extensions;
using Application.Contracts;
using Contracts;
using Diagnostics;
using Fluxor;
using Kernel;
using Microsoft.Extensions.Logging;
using Store;

public sealed class ReplayAgent : IReplayAgent, IDisposable
{
    private readonly ILogger<ReplayAgent> _logger;
    private readonly IDispatcher _dispatcher;
    private readonly IEventBus _eventBus;
    private readonly IPropertiesManager _properties;
    private readonly ITime _time;

    public ReplayAgent(
        ILogger<ReplayAgent> logger,
        IDispatcher dispatcher,
        IEventBus eventBus,
        IPropertiesManager properties,
        ITime time)
    {
        _logger = logger;
        _dispatcher = dispatcher;
        _eventBus = eventBus;
        _properties = properties;
        _time = time;

        SubscribeToEvents();
    }

    public void Dispose()
    {
        _eventBus.UnsubscribeAll(this);
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<GameDiagnosticsStartedEvent>(this, Handle);
    }

    private void Handle(GameDiagnosticsStartedEvent evt)
    {
        if (evt.Context is not ReplayContext context)
        {
            return;
        }

        var game = _properties.GetValues<IGameDetail>(GamingConstants.AllGames).FirstOrDefault(g => g.Id == evt.GameId);
        if (game == null)
        {
            return;
        }

        var gameName = game.ThemeName;
        if (!string.IsNullOrEmpty(game.VariationId))
        {
            gameName = $"{gameName} ({game.VariationId})";
        }

        var label = evt.Label;

        var sequence = context.Arguments.LogSequence;

        var startTime = context.GameIndex == -1 || context.GameIndex == 0
            ? _time.GetLocationTime(context.Arguments.StartDateTime)
            : _time.GetLocationTime(context.Arguments.FreeGames.ElementAt(context.GameIndex - 1)?.StartDateTime ?? DateTime.MinValue);

        var endCredits = context.Arguments.EndCredits;

        var gameWinBonuses = context.Arguments.GameWinBonus.CentsToDollars();

        var voucherOut = context.GameIndex == -1 || context.GameIndex == 0
            ? context.Arguments.Transactions
                .Where(t => t.TransactionType == typeof(VoucherOutTransaction))
                .Sum(t => t.Amount)
            : context.Arguments.FreeGames.ElementAt(context.GameIndex - 1)?.AmountOut ?? 0;

        var harderMeterOut = context.GameIndex == -1 || context.GameIndex == 0
            ? context.Arguments.Transactions
                .Where(t => t.TransactionType == typeof(HardMeterOutTransaction))
                .Sum(t => t.Amount)
            : context.Arguments.FreeGames.ElementAt(context.GameIndex - 1)?.AmountOut ?? 0;

        var handpayTransactions = context.Arguments.Transactions
         .Where(
             t => t.TransactionType == typeof(HandpayTransaction) && t.Amount > 0 && t.HandpayType != null)
         .ToList();

        var bonusOrGameWinToCredits = 0L;
        var bonusOrGameWinToHandpay = 0L;
        var cancelledCredits = 0L;

        if (context.GameIndex > 0)
        {
            foreach (var handpayTransaction in handpayTransactions)
            {
                switch (handpayTransaction.HandpayType)
                {
                    case HandpayType.GameWin:
                    case HandpayType.BonusPay:
                        switch (handpayTransaction.KeyOffType)
                        {
                            case KeyOffType.LocalCredit:
                            case KeyOffType.RemoteCredit:
                                bonusOrGameWinToCredits += handpayTransaction.Amount;
                                break;
                            case KeyOffType.LocalHandpay:
                            case KeyOffType.RemoteHandpay:
                                bonusOrGameWinToHandpay += handpayTransaction.Amount;
                                break;
                        }
                        break;

                    case HandpayType.CancelCredit:
                        cancelledCredits += handpayTransaction.Amount;
                        break;
                }
            }
        }

        _dispatcher.Dispatch(new ReplayStartedAction(
            gameName,
            label,
            sequence,
            startTime,
            endCredits,
            gameWinBonuses,
            voucherOut,
            harderMeterOut,
            bonusOrGameWinToCredits,
            bonusOrGameWinToHandpay,
            cancelledCredits));
    }
}

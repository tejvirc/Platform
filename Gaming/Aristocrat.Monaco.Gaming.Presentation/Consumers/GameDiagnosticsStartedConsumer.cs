namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Accounting.Contracts;
using Accounting.Contracts.HandCount;
using Accounting.Contracts.Handpay;
using Application.Contracts;
using Application.Contracts.Extensions;
using Diagnostics;
using Extensions.Fluxor;
using Fluxor;
using Gaming.Contracts;
using Kernel;
using Microsoft.Extensions.Logging;
using Store;

public class GameDiagnosticsStartedConsumer : Consumes<GameDiagnosticsStartedEvent>
{
    private readonly ILogger<GameDiagnosticsStartedConsumer> _logger;
    private readonly IDispatcher _dispatcher;
    private readonly IPropertiesManager _properties;
    private readonly ITime _time;

    public GameDiagnosticsStartedConsumer(
       ILogger<GameDiagnosticsStartedConsumer> logger,
       IDispatcher dispatcher,
       IPropertiesManager properties,
       ITime time)
    {
        _logger = logger;
        _dispatcher = dispatcher;
        _properties = properties;
        _time = time;
    }

    public override async Task ConsumeAsync(GameDiagnosticsStartedEvent theEvent, CancellationToken cancellationToken)
    {
        if (theEvent.Context is not ReplayContext context)
        {
            return;
        }

        var game = _properties.GetValues<IGameDetail>(GamingConstants.AllGames).FirstOrDefault(g => g.Id == theEvent.GameId);
        if (game == null)
        {
            return;
        }

        var gameName = game.ThemeName;
        if (!string.IsNullOrEmpty(game.VariationId))
        {
            gameName = $"{gameName} ({game.VariationId})";
        }

        var label = theEvent.Label;

        var sequence = context.Arguments.LogSequence;

        var startTime = context.GameIndex == -1 || context.GameIndex == 0
            ? _time.GetLocationTime(context.Arguments.StartDateTime)
            : _time.GetLocationTime(context.Arguments.FreeGames.ElementAt(context.GameIndex - 1)?.StartDateTime ?? DateTime.MinValue);

        var endCredits = context.Arguments.EndCredits;

        var gameWinBonuses = context.Arguments.GameWinBonus.CentsToDollars();

        var voucherOutL = context.GameIndex == -1 || context.GameIndex == 0
            ? context.Arguments.Transactions
                .Where(t => t.TransactionType == typeof(VoucherOutTransaction))
                .Sum(t => t.Amount)
            : context.Arguments.FreeGames.ElementAt(context.GameIndex - 1)?.AmountOut ?? 0;

        var voucherOut = 0m;

        if (voucherOutL > 0)
        {
            voucherOut = Convert.ToDecimal(voucherOutL / GamingConstants.Millicents) / CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit;
        }

        var harderMeterOut = (context.GameIndex == -1 || context.GameIndex == 0
            ? context.Arguments.Transactions
                .Where(t => t.TransactionType == typeof(HardMeterOutTransaction))
                .Sum(t => t.Amount)
            : context.Arguments.FreeGames.ElementAt(context.GameIndex - 1)?.AmountOut ?? 0)
            .MillicentsToDollars();

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

        await _dispatcher.DispatchAsync(new ReplayStartedAction(
            gameName,
            label,
            sequence,
            startTime,
            endCredits,
            gameWinBonuses,
            voucherOut,
            harderMeterOut,
            bonusOrGameWinToCredits.MillicentsToDollars(),
            bonusOrGameWinToHandpay.MillicentsToDollars(),
            cancelledCredits.MillicentsToDollars()));
    }
}

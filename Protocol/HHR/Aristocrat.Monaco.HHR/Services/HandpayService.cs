namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Gaming.Contracts.Payment;
    using Client.Messages;
    using Client.WorkFlow;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Storage.Helpers;
    using HandpayType = Accounting.Contracts.Handpay.HandpayType;

    /// <summary>
    ///     This service exists to register as a PaymentDetermination delegate and to subscribe to handpay and voucher out
    ///     events so that we can split large wins as per the HHR rules and then send messages to the HHR server as amounts
    ///     are keyed off by an operator.
    /// </summary>
    public class HandpayService : IPaymentDeterminationHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const uint HandpayTypeProgressive = (uint)Client.Messages.HandpayType.HandpayTypeProgressive;
        private const uint HandpayTypeNonProgressive = (uint)Client.Messages.HandpayType.HandpayTypeNonProgressive;

        private readonly ICentralManager _centralManager;
        private readonly IPropertiesManager _properties;
        private readonly IPlayerSessionService _playerSession;
        private readonly IPlayerBank _playerBank;
        private readonly IPrizeInformationEntityHelper _prizeInformationEntityHelper;
        private readonly ITransactionIdProvider _transactionIdProvider;

        /// <summary>
        ///     Implement the <see cref="IPaymentDeterminationHandler" /> interface, so we may split large wins as required
        ///     by HHR, and register for handpay/voucher events so that we may send handpay messages as required by the spec.
        /// </summary>
        /// <param name="centralManager">The HHR central manager, used for sending messages to the server</param>
        /// <param name="properties">The property provider, where we can find configured IRS limits for handpay</param>
        /// <param name="playerSession">The player ID service, so we can get a player ID for the messages we send</param>
        /// <param name="playerBank">The player bank which tells us how much credit is currently on the meter</param>
        /// <param name="eventBus">The event bus, where we can listen for the handpay completion events</param>
        /// <param name="largeWinDetermination">The payment determination provider, where we will register ourselves</param>
        /// <param name="prizeInformationEntityHelper">A class that facilitates the getting and setting of the prize information</param>
        /// <param name="gamePlayState">Allows us to check the current game state, to check if we are in a game.</param>
        /// <param name="transactionIdProvider">Provides with next transaction Id</param>
        public HandpayService(
            ICentralManager centralManager,
            IPropertiesManager properties,
            IPlayerSessionService playerSession,
            IPlayerBank playerBank,
            IEventBus eventBus,
            IPaymentDeterminationProvider largeWinDetermination,
            IPrizeInformationEntityHelper prizeInformationEntityHelper,
            ITransactionIdProvider transactionIdProvider)
        {
            _centralManager = centralManager ?? throw new ArgumentNullException(nameof(centralManager));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _playerSession = playerSession ?? throw new ArgumentNullException(nameof(playerSession));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
            _prizeInformationEntityHelper = prizeInformationEntityHelper ?? throw new ArgumentNullException(nameof(prizeInformationEntityHelper));
            _transactionIdProvider = transactionIdProvider ?? throw new ArgumentNullException(nameof(transactionIdProvider));

            // Register as the guy who will make decisions about what to do with large wins in PayGameResultsCommandHandler.
            largeWinDetermination.Handler = this;

            // Subscribe for the events that will fire when handpays are cleared.
            eventBus.Subscribe<HandpayCompletedEvent>(this, HandleHandpayCompleted);
        }

        /// <inheritdoc />
        public List<PaymentDeterminationResult> GetPaymentResults(long winInMillicents, bool isPayGameResults=true)
        {
            return LargeWinCheck(Guid.Empty, winInMillicents, isPayGameResults);
        }

        /// <summary>
        ///     Deals with the result of a handpay that we've previously caused to happen. Sends all the messages required
        ///     by the protocol indicating what happened to each amount.
        /// </summary>
        /// <param name="handpayEvent">The HandpayCompletedEvent object which tells us what happened to the money.</param>
        /// <returns>
        ///     A list of PaymentDeterminationResult objects, which won't be used in our case because we've already done that.
        /// </returns>
        private void HandleHandpayCompleted(HandpayCompletedEvent handpayEvent)
        {
            if (handpayEvent.Transaction.HandpayType == HandpayType.GameWin)
            {
                Logger.Debug($"Handpay Completed for GameWin and handpayId :{handpayEvent.Transaction.TraceId} ");
                LargeWinCheck(handpayEvent.Transaction.TraceId, 0, true, handpayEvent.Transaction.KeyOffType);
            }
        }

        /// <summary>
        ///     Performs the calculations specific to Kentucky/HHR to figure out the amounts that need to be handpaid.
        /// </summary>
        /// <param name="handpayTraceId">
        ///     Indicates whether we are doing this to determine the handpay lockup amount, or doing it to send messages once
        ///     we know the outcome of the handpay lockup. The return value is the same either way.
        /// </param>
        /// <param name="amountMillis">
        ///     The amount of the win in millicents, which is only used if we can't find any information about the amount we
        ///     are paying from a game win.
        /// </param>
        /// <param name="isPayGameResults">
        ///     Is used to indicate whether we are doing a full prize calculation (e.g. for PayGameResults) or whether we
        ///     just need to know whether there will be a handpay happening (e.g. for PresentEventHandler). This is a HHR
        ///     thing that most other people should be able to ignore.
        /// </param>
        /// <returns>
        ///     A list of PaymentDeterminationResult objects, which tells us how much money to put on the credit meter and how
        ///     much to handpay.
        /// </returns>
        private List<PaymentDeterminationResult> LargeWinCheck(Guid handpayTraceId, long amountMillis, bool isPayGameResults = true, KeyOffType keyOffType = KeyOffType.Unknown)
        {
            List<PaymentDeterminationResult> listOfHandpays = new List<PaymentDeterminationResult>();

            // Note that if getting this object fails, we will just let the exception go up, since this
            // would indicate a major failure of some sort: one which we cannot recover from.
            PrizeInformation prizeInfo = _prizeInformationEntityHelper.PrizeInformation;

            // If there is no prize information then we can't possibly be paying a game, so just indicate that we would
            // like to pay the amount to the credit meter, in case someone else has messed up.
            if (prizeInfo == null)
            {
                listOfHandpays.Add(new PaymentDeterminationResult(amountMillis, 0, Guid.NewGuid()));
                return listOfHandpays;
            }

            var ctx = new HandpayServiceContext(_properties, prizeInfo)
            {
                // Get the current credit meter in cents, as we'll need this amount as we go. If there is no handpay we can
                // update this value as we determine they can be credited. If there is a handpay, we have to wait until it
                // is cleared before we can send the messages, so we'll just want the current value.
                CurrentCreditMeterCents = _playerBank.Balance.MillicentsToCents()
            };

            if (ctx.RaceSet1TotalWinCents > 0)
            {
                ProcessRaceSet1(ctx, listOfHandpays, handpayTraceId, keyOffType, isPayGameResults);
            }

            if (ctx.RaceSet2TotalWinCents > 0)
            {
                ProcessRaceSet2(ctx, listOfHandpays, handpayTraceId, keyOffType, isPayGameResults);
            }

            return listOfHandpays;
        }

        private void ProcessRaceSet1(HandpayServiceContext ctx, List<PaymentDeterminationResult> result, Guid handpayTraceId, KeyOffType keyOffType, bool isPayGameResults = true)
        {
            // Did we win more than the IRS limit for this win? If so, we'll need to send all the messages required by HHR,
            // otherwise, we have to send the "normal" messages indicating a credit meter increment for the prize.
            if (DoesWinAmountRequireHandpay(ctx, ctx.RaceSet1NetPrizeCents, ctx.RaceSet1WagerCents))
            {
                PrizeInformation prizeInfo = ctx.PrizeInfo;
                if (handpayTraceId == Guid.Empty)
                {
                    // So there's no handpay event, meaning we are currently determining whether or not we need to handpay, and
                    // we will be called again later with the result of the handpay at which point we can send the messages.
                    Guid handpayGuid = Guid.NewGuid();
                    result.Add(new PaymentDeterminationResult(ctx.RaceSet1WagerCents, ctx.RaceSet1NetPrizeCents, handpayGuid, true));

                    // If we're just checking whether there's a handpay for the PendingHandpay flag for GDK, stop here and don't
                    // mess around with persistence and such.
                    if (!isPayGameResults)
                    {
                        return;
                    }

                    // Otherwise what we need to do is to record the Guid that we've attached to this handpay, so we can check it
                    // later when the handpay is keyed off. This has to be serialized and saved over the current value.
                    prizeInfo.RaceSet1HandpayGuid = handpayGuid;
                    prizeInfo.RaceSet1HandpayKeyedOff = false;

                    _prizeInformationEntityHelper.PrizeInformation = prizeInfo;

                    // One last thing, record the wager amount so that the accounting layer can display and print it.
                    _properties.SetProperty(ApplicationConstants.LastWagerWithLargeWinInfo, ((long)ctx.RaceSet1WagerCents).CentsToMillicents());

                    return;
                }

                // OK so we know that someone is keying off a handpay, but is it *this* handpay, and are we sure we haven't
                // already dealt with this handpay?
                if (handpayTraceId != prizeInfo.RaceSet1HandpayGuid || prizeInfo.RaceSet1HandpayKeyedOff) return;

                CommandTransactionType commandTransactionType;
                if (keyOffType == KeyOffType.LocalCredit || keyOffType == KeyOffType.RemoteCredit)
                {
                    commandTransactionType = CommandTransactionType.GameWinToCreditMeter;
                }
                else
                {
                    commandTransactionType = CommandTransactionType.GameWinToHandpayNoReceipt;
                }

                // Send the game win to credit meter message for the wager part. Note that this amount will already be on
                // the credit meter.
                SendGameWinTransaction(ctx, CommandTransactionType.GameWinToCreditMeter, ctx.RaceSet1WagerCents, HandpayTypeNonProgressive);

                SendGameWinTransaction(ctx, commandTransactionType, ctx.RaceSet1NetPrizeCents, HandpayTypeNonProgressive);

                SendHandpayCreateRequest(ctx, ctx.RaceSet1NetPrizeCents, ctx.RaceSet1TotalWinCents, 0, ctx.RaceSet1WagerCents, HandpayTypeNonProgressive);

                // We need to record that we've keyed off this handpay now, so that we won't double handle it.
                prizeInfo.RaceSet1HandpayKeyedOff = true;

                _prizeInformationEntityHelper.PrizeInformation = prizeInfo;
            }
            else
            {
                // The result we're returning will indicate there's no handpay for this win, so we can send the game win
                // messages right now. If we are later called with a handpay result then we know it can't have been for this.
                if (handpayTraceId != Guid.Empty)
                {
                    return;
                }

                result.Add(new PaymentDeterminationResult(ctx.RaceSet1TotalWinCents, 0, Guid.Empty, true));
                if (!isPayGameResults)
                {
                    return;
                }

                // As discussed with the Ainsworth team, send GameWinToCreditMeter transaction even Current Credit + Win  > Credit Limit, the extra credit
                //would be either canceled credit or Cashed out and the appropriate message would be sent to the HHR Server in CreditOutService.

                /* if (IsWinOverCreditLimit(ctx.CurrentCreditMeterCents + ctx.RaceSet1TotalWinCents + ctx.RaceSet2TotalWinCents))
                {
                    return;
                }*/

                // Send the game win to credit meter message for the "non large" win.
                ctx.CurrentCreditMeterCents += ctx.RaceSet1TotalWinCents;

                SendGameWinTransaction(ctx, CommandTransactionType.GameWinToCreditMeter, ctx.RaceSet1TotalWinCents, HandpayTypeNonProgressive);
            }
        }

        private void ProcessRaceSet2(HandpayServiceContext ctx, List<PaymentDeterminationResult> result, Guid handpayTraceId, KeyOffType keyOffType, bool isPayGameResults = true)
        {
            // We need to figure out if there was a progressive win. This is as per the sample code from HHR, and
            // it is non-trivial to follow.
            if (DoesWinAmountRequireHandpay(ctx, ctx.RaceSet2NetPrizeCents, ctx.RaceSet2WagerCents))
            {
                PrizeInformation prizeInfo = ctx.PrizeInfo;
                if (handpayTraceId == Guid.Empty)
                {
                    // So there's no handpay event, meaning we are currently determining whether or not we need to handpay, and
                    // we will be called again later with the result of the handpay at which point we can send the messages.
                    Guid handpayGuid = Guid.NewGuid();
                    result.Add(new PaymentDeterminationResult(ctx.RaceSet2WagerCents, ctx.RaceSet2NetPrizeCents, handpayGuid, true));

                    // If we're just checking whether there's a handpay for the PendingHandpay flag for GDK, stop here and don't
                    // mess around with persistence and such.
                    if (!isPayGameResults)
                    {
                        return;
                    }

                    // Otherwise what we need to do is to record the Guid that we've attached to this handpay, so we can check it
                    // later when the handpay is keyed off. This has to be serialized and saved over the current value.
                    prizeInfo.RaceSet2HandpayGuid = handpayGuid;
                    prizeInfo.RaceSet2HandpayKeyedOff = false;

                    _prizeInformationEntityHelper.PrizeInformation = prizeInfo;

                    // One last thing, record the wager amount so that the accounting layer can display and print it.
                    _properties.SetProperty(ApplicationConstants.LastWagerWithLargeWinInfo, ((long)ctx.RaceSet2WagerCents).CentsToMillicents());

                    return;
                }

                // OK so we know that someone is keying off a handpay, but is it *this* handpay, and are we sure we haven't
                // already dealt with this handpay?
                if (handpayTraceId != prizeInfo.RaceSet2HandpayGuid || prizeInfo.RaceSet2HandpayKeyedOff) return;

                CommandTransactionType commandTransactionType;
                if (keyOffType == KeyOffType.LocalCredit || keyOffType == KeyOffType.RemoteCredit)
                {
                    commandTransactionType = CommandTransactionType.GameWinToCreditMeter;
                }
                else
                {
                    commandTransactionType = CommandTransactionType.GameWinToHandpayNoReceipt;
                }

                // If we won enough on the non-progressive part of the game, then just as for race set 1, we pay back the
                // wager amount to the credit meter as a non-progressive handpay. Otherwise, we will have to use part of
                // our progressive win for the wager part of the handpay.
                if (ctx.RaceSet2NonProgressiveNetPrizeCents > 0)
                {
                    SendGameWinTransaction(ctx, CommandTransactionType.GameWinToCreditMeter, ctx.RaceSet2WagerCents, HandpayTypeNonProgressive);
                    SendGameWinTransaction(ctx, commandTransactionType, ctx.RaceSet2NonProgressiveNetPrizeCents, HandpayTypeNonProgressive);
                }
                else
                {
                    // According to the HHR code, we can only get here if there was a progressive win. What they really
                    // mean is you can only get here if you have won a large amount and the race set win was smaller than
                    // your wager. This means you have to do two separate transactions to repay the wager amount.
                    ProcessSplitWagerRepay(ctx);
                }

                // And finally we can handpay the progressive winnings part
                if (ctx.RaceSet2ProgressiveTotalWinCents > 0)
                {
                    // If we had a deficit of non-progressive winnings for the credit meter, we need to make sure we account
                    // for that amount.
                    var progressiveNetPrizeCents = ctx.RaceSet2ProgressiveTotalWinCents;
                    if (ctx.RaceSet2NonProgressiveNetPrizeCents < 0)
                    {
                        progressiveNetPrizeCents += ctx.RaceSet2NonProgressiveNetPrizeCents;
                    }

                    SendGameWinTransaction(ctx, commandTransactionType, progressiveNetPrizeCents, HandpayTypeProgressive);

                    SendHandpayCreateRequest(ctx, ctx.RaceSet2NetPrizeCents, 0, ctx.RaceSet2ProgressiveTotalWinCents, ctx.RaceSet2WagerCents, HandpayTypeProgressive);
                }
                else
                {
                    // The only thing that can be left is if we had to do a handpay but the progressive part of the win was
                    // zero, so we need to send a non-progressive handpay message. This isn't mutually exclusive of the code
                    // above, but it's the only message we have left to send.
                    SendHandpayCreateRequest(ctx, ctx.RaceSet2NetPrizeCents, ctx.RaceSet2NonProgressiveTotalWinCents, 0, ctx.RaceSet2WagerCents, HandpayTypeNonProgressive);
                }

                // We need to record that we've keyed off this handpay now, so that we won't double handle it.
                prizeInfo.RaceSet2HandpayKeyedOff = true;

                _prizeInformationEntityHelper.PrizeInformation = prizeInfo;
            }
            else
            {
                // The result we're returning will indicate there's no handpay for this win, so we can send the game win
                // messages right now. If we are later called with a handpay result then we know it can't have been for this.
                if (handpayTraceId != Guid.Empty)
                {
                    return;
                }

                result.Add(new PaymentDeterminationResult(ctx.RaceSet2TotalWinCents, 0, Guid.Empty, true));
                if (!isPayGameResults)
                {
                    return;
                }

                // As discussed with the Ainsworth team, send GameWinToCreditMeter transaction even Current Credit + Win  > Credit Limit, the extra credit
                //would be either canceled credit or Cashed out and the appropriate message would be sent to the HHR Server in CreditOutService.

                /*if (IsWinOverCreditLimit(ctx.CurrentCreditMeterCents + ctx.RaceSet1TotalWinCents + ctx.RaceSet2TotalWinCents))
                {
                    return;
                }*/

                // Send the game win to credit meter message for the "non large" wins we had
                if (ctx.RaceSet2NonProgressiveTotalWinCents > 0)
                {
                    ctx.CurrentCreditMeterCents += ctx.RaceSet2NonProgressiveTotalWinCents;
                    SendGameWinTransaction(ctx, CommandTransactionType.GameWinToCreditMeter, ctx.RaceSet2NonProgressiveTotalWinCents, HandpayTypeNonProgressive);
                }

                if (ctx.RaceSet2ProgressiveTotalWinCents > 0)
                {
                    ctx.CurrentCreditMeterCents += ctx.RaceSet2ProgressiveTotalWinCents;
                    SendGameWinTransaction(ctx, CommandTransactionType.GameWinToCreditMeter, ctx.RaceSet2ProgressiveTotalWinCents, HandpayTypeProgressive);
                }
            }
        }

        private void ProcessSplitWagerRepay(HandpayServiceContext ctx)
        {
            // First, pay back the wager to the credit meter using whatever small amount we won from the race set
            if (ctx.RaceSet2NonProgressiveTotalWinCents > 0)
            {
                SendGameWinTransaction(ctx, CommandTransactionType.GameWinToCreditMeter, ctx.RaceSet2NonProgressiveTotalWinCents, HandpayTypeNonProgressive);
            }

            // Next, pay back whatever handpay we still owe on the wager amount from our progressive win amount
            var progressiveWagerRepay = ctx.RaceSet2WagerCents - ctx.RaceSet2NonProgressiveTotalWinCents;
            if (progressiveWagerRepay > 0)
            {
                SendGameWinTransaction(ctx, CommandTransactionType.GameWinToCreditMeter, progressiveWagerRepay, HandpayTypeProgressive);
            }
        }

        private static bool DoesWinAmountRequireHandpay(
            HandpayServiceContext ctx,
            decimal prizeCents,
            uint wagerCents)
        {
            return prizeCents >= ctx.WinLimitCents && prizeCents / wagerCents >= ctx.WinLimitRatio;
        }

        // Send the message to transfer the wager amount to the credit meter or to indicate handpay of the IRS lockup amount
        private async void SendGameWinTransaction(
            HandpayServiceContext ctx,
            CommandTransactionType transactionType,
            long amountCents,
            uint handpayType)
        {
            try
            {
                await _centralManager.Send<TransactionRequest, CloseTranResponse>(
                    new TransactionRequest
                    {
                        TransactionType = transactionType,
                        TransactionId = (uint) _transactionIdProvider.GetNextTransactionId(),
                        PlayerId = await _playerSession.GetCurrentPlayerId(),
                        Credit = (uint)amountCents,
                        CashBalance = (uint)ctx.CurrentCreditMeterCents,
                        GameMapId = ctx.PrizeInfo.GameMapId,
                        LastGamePlayTime = ctx.PrizeInfo.LastGamePlayedTime,
                        HandpayType = handpayType,
                        RequestTimeout = new LockupRequestTimeout
                        {
                            LockupKey = HhrConstants.GameWinCmdTransactionErrorKey,
                            LockupString = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameWinTransactionCmdFailedMsg),
                            LockupHelpText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameWinTransactionCmdFailedHelpMsg)
                        },
                        TimeoutInMilliseconds = HhrConstants.MsgTransactionTimeoutMs,
                        RetryCount = HhrConstants.RetryCount
                    });
            }
            catch (UnexpectedResponseException ex)
            {
                Logger.Error("Unexpected response for " + transactionType + " message", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("Unknown error for " + transactionType + " message", ex);
            }
        }

        // Send the message to create the handpay for the amount that was won
        private async void SendHandpayCreateRequest(
            HandpayServiceContext ctx,
            long prizeCents,
            long gameWinCents,
            long progWinCents,
            uint wagerCents,
            uint handpayType)
        {
            try
            {
                await _centralManager.Send<HandpayCreateRequest, CloseTranResponse>(
                    new HandpayCreateRequest
                    {
                        TransactionId = (uint) _transactionIdProvider.GetNextTransactionId(),
                        HandpayType = handpayType,
                        Amount = (uint)prizeCents,
                        Denomination = ctx.PrizeInfo.Denomination,
                        GameWin = (uint)gameWinCents,
                        ProgWin = (uint)progWinCents,
                        PlayerId = await _playerSession.GetCurrentPlayerId(),
                        LastWager = wagerCents,
                        GameMapId = ctx.PrizeInfo.GameMapId,
                        LastGamePlayTime = ctx.PrizeInfo.LastGamePlayedTime,
                        // NOTE: We have disabled this as we aren't getting any response from the server. This is an outstanding
                        // issue that we have asked Ainsworth for help with. UPDATE: They have fixed the issue in latest version
                        // of the server, however we aren't sure yet when this will be deployed for customers.
                        //RequestTimeout = new LockupRequestTimeout
                        //{
                        //    LockupKey = HhrConstants.HandpayTransactionErrorKey,
                        //    LockupString = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HandpayTransactionCmdFailedMsg),
                        //    LockupHelpText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HandpayTransactionCmdFailedHelpMsg)
                        //},
                        TimeoutInMilliseconds = HhrConstants.MsgTransactionTimeoutMs,
                        RetryCount = HhrConstants.RetryCount
                    });
            }
            catch (UnexpectedResponseException ex)
            {
                Logger.Error("Unexpected response for HandpayCreateRequest message", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("Unknown error for HandpayCreateRequest message", ex);
            }
        }
    }
}
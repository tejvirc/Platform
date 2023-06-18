namespace Aristocrat.Monaco.Gaming.UI
{
    using Accounting.Contracts.Handpay;
    using Contracts;
    using Contracts.Models;
    using Runtime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Extensions;
    using log4net;
    using Utils;
    using ViewModels;

    public class GameControlledOverlayMessageStrategy : IOverlayMessageStrategy
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IOverlayMessageStrategyController _overlayMessageStrategyController;
        private readonly IPresentationService _presentationService;

        public long LastCashOutAmount { get; set; }

        public long HandpayAmount { get; set; }

        public long LargeWinWager { get; set; }

        public HandpayType? LastHandpayType { get; set; }

        public bool CashOutButtonPressed { get; set; }

        public bool IsBasic => false;

        public GameControlledOverlayMessageStrategy(IOverlayMessageStrategyController overlayMessageStrategyController, IPresentationService presentationService)
        {
            _overlayMessageStrategyController = overlayMessageStrategyController ??
                                                throw new ArgumentNullException(
                                                    nameof(overlayMessageStrategyController));
            _presentationService = presentationService ?? throw new ArgumentNullException(nameof(presentationService));
        }

        public IMessageOverlayData HandleMessageOverlayCashOut(
            IMessageOverlayData data,
            bool lastCashOutForcedByMaxBank,
            LobbyCashOutState cashOutState)
        {
            var registeredPresentations = _overlayMessageStrategyController.RegisteredPresentations;
            if (!_overlayMessageStrategyController.GameRegistered)
            {
                data = _overlayMessageStrategyController.FallBackStrategy.HandleMessageOverlayCashOut(
                    data,
                    lastCashOutForcedByMaxBank,
                    cashOutState);
                return data;
            }

            var overriddenPresentations = new List<PresentationOverrideTypes>();

            switch (cashOutState)
            {
                case LobbyCashOutState.Voucher when registeredPresentations.Contains(PresentationOverrideTypes.PrintingCashwinTicket) && lastCashOutForcedByMaxBank:
                    overriddenPresentations.Add(PresentationOverrideTypes.PrintingCashwinTicket);
                    break;
                case LobbyCashOutState.Voucher when registeredPresentations.Contains(PresentationOverrideTypes.PrintingCashoutTicket) && !lastCashOutForcedByMaxBank:
                    overriddenPresentations.Add(PresentationOverrideTypes.PrintingCashoutTicket);
                    break;
                case LobbyCashOutState.Wat when registeredPresentations.Contains(PresentationOverrideTypes.TransferingOutCredits) && !lastCashOutForcedByMaxBank:
                    overriddenPresentations.Add(PresentationOverrideTypes.TransferingOutCredits);
                    break;
                case LobbyCashOutState.HandPay when registeredPresentations.Contains(PresentationOverrideTypes.JackpotHandpay):
                case LobbyCashOutState.Undefined:
                    // Avoid calling the fallback strategy when the presentation will be handled by the game later.
                    break;
                default:
                    data = _overlayMessageStrategyController.FallBackStrategy.HandleMessageOverlayCashOut(
                        data,
                        lastCashOutForcedByMaxBank,
                        cashOutState);
                    break;
            }

            if (LastCashOutAmount <= 0)
            {
                Logger.Warn($"HandleMessageOverlayCashOut {cashOutState} - LastCashOutAmount {LastCashOutAmount}, presentation not overriden");
                return data;
            }

            if (overriddenPresentations.Any())
            {
                var gameDrivenData = new MessageOverlayData();
                gameDrivenData = (MessageOverlayData)_overlayMessageStrategyController.FallBackStrategy.HandleMessageOverlayCashOut(
                    gameDrivenData,
                    lastCashOutForcedByMaxBank,
                    cashOutState);
                var message = GeneratePresentationMessage(
                    gameDrivenData.Text,
                    gameDrivenData.SubText,
                    gameDrivenData.SubText2);

                Logger.Debug("Sending PresentOverriddenPresentation with message: " + message);

                var amount = OverlayMessageUtils.ToCredits(LastCashOutAmount).FormattedCurrencyString();
                var presentations = overriddenPresentations.Select(presentation => new PresentationOverrideData(message, amount, presentation)).ToList();

                _presentationService.PresentOverriddenPresentation(presentations);
            }

            return data;
        }

        public IMessageOverlayData HandleMessageOverlayCashIn(
            IMessageOverlayData data,
            CashInType cashInType,
            bool stateContainsCashOut,
            LobbyCashOutState cashOutState)
        {
            if (!_overlayMessageStrategyController.GameRegistered)
            {
                data = _overlayMessageStrategyController.FallBackStrategy.HandleMessageOverlayCashIn(
                    data, cashInType, stateContainsCashOut, cashOutState);
                return data;
            }

            var overriddenPresentations = new List<PresentationOverrideTypes>();
            var registeredPresentations = _overlayMessageStrategyController.RegisteredPresentations;
            if (registeredPresentations.Contains(PresentationOverrideTypes.TransferingInCredits))
            {
                overriddenPresentations.Add(PresentationOverrideTypes.TransferingInCredits);
                var gameDrivenData = new MessageOverlayData();
                gameDrivenData = (MessageOverlayData)_overlayMessageStrategyController.FallBackStrategy.HandleMessageOverlayCashIn(
                    gameDrivenData, cashInType, stateContainsCashOut, cashOutState);
                var message = GeneratePresentationMessage(
                    gameDrivenData.Text,
                    gameDrivenData.SubText,
                    gameDrivenData.SubText2);

                Logger.Debug("Sending PresentOverriddenPresentation with message: " + message);
                
                var presentations = overriddenPresentations.Select(presentation => new PresentationOverrideData(message, presentation)).ToList();

                _presentationService.PresentOverriddenPresentation(presentations);
            }
            else
            {
                data = _overlayMessageStrategyController.FallBackStrategy.HandleMessageOverlayCashIn(
                    data, cashInType, stateContainsCashOut, cashOutState);
            }

            return data;
        }

        public IMessageOverlayData HandleMessageOverlayHandPay(IMessageOverlayData data, string subText2)
        {
            if (!_overlayMessageStrategyController.GameRegistered)
            {
                data = _overlayMessageStrategyController.FallBackStrategy.HandleMessageOverlayHandPay(data, subText2);
                return data;
            }

            Logger.Debug("Handling Game Controlled handpay with amount: $" + HandpayAmount);

            var overriddenPresentations = new List<PresentationOverrideTypes>();
            var registeredPresentations = _overlayMessageStrategyController.RegisteredPresentations;
            switch (LastHandpayType)
            {
                case HandpayType.GameWin when registeredPresentations.Contains(PresentationOverrideTypes.JackpotHandpay):
                    overriddenPresentations.Add(PresentationOverrideTypes.JackpotHandpay);
                    break;
                case HandpayType.BonusPay when registeredPresentations.Contains(PresentationOverrideTypes.BonusJackpot):
                    overriddenPresentations.Add(PresentationOverrideTypes.BonusJackpot);
                    break;
                case HandpayType.CancelCredit when registeredPresentations.Contains(PresentationOverrideTypes.CancelledCreditsHandpay):
                    overriddenPresentations.Add(PresentationOverrideTypes.CancelledCreditsHandpay);
                    break;
                default:
                    data = _overlayMessageStrategyController.FallBackStrategy.HandleMessageOverlayHandPay(data, subText2);
                    break;
            }

            if (overriddenPresentations.Any())
            {
                var gameDrivenData = new MessageOverlayData();
                gameDrivenData = (MessageOverlayData)_overlayMessageStrategyController.FallBackStrategy.HandleMessageOverlayHandPay(gameDrivenData, subText2);
                var message = GeneratePresentationMessage(
                    gameDrivenData.Text,
                    gameDrivenData.SubText,
                    gameDrivenData.SubText2);

                Logger.Debug("Sending PresentOverriddenPresentation with message: " + message);
                
                var amount = OverlayMessageUtils.ToCredits(HandpayAmount).FormattedCurrencyString();
                var presentations = overriddenPresentations.Select(presentation => new PresentationOverrideData(message, amount, presentation)).ToList();

                _presentationService.PresentOverriddenPresentation(presentations);
            }

            if(data != null && overriddenPresentations.Any())
            {
                data.GameHandlesHandPayPresentation = true;
            }

            return data;
        }

        private static string GeneratePresentationMessage(string text, string subtext, string subtext2)
        {
            return text + "\n" + subtext + "\n" + subtext2;
        }
    }
}

namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Storage.Models;
    using Ticketing;

    /// <summary>
    ///     The handler for LP 71 Redeem Ticket
    /// </summary>
    public class LP71RedeemTicketHandler : ISasLongPollHandler<RedeemTicketResponse, RedeemTicketData>
    {
        private const string NilBarcode = "000000000";
        private readonly TicketingCoordinator _ticketingCoordinator;
        private readonly IBank _bank;
        private readonly SasValidationType _validationType;
        private RedemptionStatusCode _redemptionStatusCode;
        private readonly IHostAcknowledgementHandler _nullHandler = new HostAcknowledgementHandler();
        private readonly IHostAcknowledgementHandler _handler;
        private readonly ISasVoucherInProvider _sasVoucherInProvider;

        /// <summary>
        ///     Creates the LP71RedeemTicketHandler handler
        /// </summary>
        /// <param name="ticketingCoordinator">The ticket coordinator</param>
        /// <param name="bank">The bank</param>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="sasVoucherInProvider">The SAS voucher in provider</param>
        public LP71RedeemTicketHandler(
            ITicketingCoordinator ticketingCoordinator,
            IBank bank,
            IPropertiesManager propertiesManager,
            ISasVoucherInProvider sasVoucherInProvider)
        {
            _ticketingCoordinator = ticketingCoordinator as TicketingCoordinator ?? throw new ArgumentNullException(nameof(ticketingCoordinator));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            var propMan = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _validationType = propMan.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ValidationType;
            _sasVoucherInProvider = sasVoucherInProvider ?? throw new ArgumentNullException(nameof(sasVoucherInProvider));

            _handler = new HostAcknowledgementHandler
            {
                ImpliedAckHandler = () => _sasVoucherInProvider.RedemptionStatusAcknowledged()
            };
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.RedeemTicket
        };

        /// <inheritdoc />
        public RedeemTicketResponse Handle(RedeemTicketData data)
        {
            //If the host rejected the ticket for any reason, Deny the pending ticket.
            if (CheckForNonValidTransferCode(data.TransferCode))
            {
                _sasVoucherInProvider.DenyTicket(_redemptionStatusCode, data.TransferCode);
                return CreateTicketResponse(_redemptionStatusCode);
            }

            if (data.TransferCode == TicketTransferCode.ValidRestrictedPromotionalTicket &&
                _bank.QueryBalance(AccountType.NonCash) > 0 &&
                _ticketingCoordinator.GetData().PoolId != data.PoolId)
            {
                _redemptionStatusCode = RedemptionStatusCode.GamingMachineUnableToAcceptTransfer;
                _sasVoucherInProvider.DenyTicket(_redemptionStatusCode, data.TransferCode);
                return CreateTicketResponse(_redemptionStatusCode);
            }

            if (data.TransferCode != TicketTransferCode.RequestForCurrentTicketStatus)
            {
                switch (_sasVoucherInProvider.CurrentState)
                {
                    case SasVoucherInState.RequestPending:
                    case SasVoucherInState.AcknowledgementPending:
                        if (IsDuplicate71Command(data))
                        {
                            //Do logic for ticket status.
                            var result = _sasVoucherInProvider.RequestValidationStatus();
                            result.Handlers = _handler;
                            return result;
                        }

                        //71 was sent but it did not match the ticket for the current cycle
                        _redemptionStatusCode = RedemptionStatusCode.NotCompatibleWithCurrentRedemptionCycle;
                        return CreateTicketResponse(RedemptionStatusCode.NotCompatibleWithCurrentRedemptionCycle);

                    case SasVoucherInState.ValidationDataPending:
                        if (data.ParsingCode != ParsingCode.Bcd)
                        {
                            _sasVoucherInProvider.DenyTicket(RedemptionStatusCode.NotAValidTransferAmount, data.TransferCode);
                            return CreateTicketResponse(RedemptionStatusCode.NotAValidTransferAmount);
                        }

                        break;

                    default:
                        //Tried to send a 71 
                        return CreateTicketResponse(RedemptionStatusCode.NotCompatibleWithCurrentRedemptionCycle);
                }
            }
            else
            {
                var result = _sasVoucherInProvider.RequestValidationStatus();
                result.Handlers = _handler;
                return result;
            }

            RedeemTicketResponse response;
            if (_sasVoucherInProvider.RedemptionEnabled &&
                (_validationType == SasValidationType.SecureEnhanced || _validationType == SasValidationType.System))
            {
                GetProperRedemptionStatusCode(data);
                _sasVoucherInProvider.AcceptTicket(data, _redemptionStatusCode);

                // Use the persisted information as we have saved if we are querying status and we only want to query persisted information
                response = CreateTicketResponse(_redemptionStatusCode);
            }
            else
            {
                _redemptionStatusCode = RedemptionStatusCode.TicketRedemptionDisabled;
                _sasVoucherInProvider.DenyTicket(_redemptionStatusCode, data.TransferCode);
                return CreateTicketResponse(_redemptionStatusCode);
            }

            return response;
        }

        private void GetProperRedemptionStatusCode(RedeemTicketData data)
        {
            if(data.Barcode != _sasVoucherInProvider.CurrentTicketInfo.Barcode)
            {
                _redemptionStatusCode = RedemptionStatusCode.ValidationNumberDoesNotMatch;
                return;
            }

            switch (data.TransferCode)
            {
                case TicketTransferCode.ValidCashableTicket:
                case TicketTransferCode.ValidNonRestrictedPromotionalTicket:
                    _redemptionStatusCode = RedemptionStatusCode.TicketRedemptionPending;
                    break;
                case TicketTransferCode.ValidRestrictedPromotionalTicket:
                    _redemptionStatusCode = RedemptionStatusCode.TicketRedemptionPending;
                    var ticketStorageData = _ticketingCoordinator.GetData();
                    var restrictedExpiration = (data.RestrictedExpiration == 0
                        ? (int)_ticketingCoordinator.DefaultTicketExpirationRestricted
                        : (int)data.RestrictedExpiration);
                    ticketStorageData.SetRestrictedExpirationWithPriority(
                        restrictedExpiration,
                        (int)_ticketingCoordinator.TicketExpirationRestricted,
                        _bank.QueryBalance(AccountType.NonCash));
                    _ticketingCoordinator.Save(ticketStorageData).FireAndForget();
                    break;
            }
        }

        private bool CheckForNonValidTransferCode(TicketTransferCode code)
        {
            bool isNonValidTicket = true;
            switch(code)
            {
                case TicketTransferCode.UnableToValidate:
                case TicketTransferCode.NotAValidValidationNumber:
                case TicketTransferCode.ValidationNumberNotInSystem:
                case TicketTransferCode.TicketMarkedPendingInSystem:
                case TicketTransferCode.TicketAlreadyRedeemed:
                case TicketTransferCode.TicketExpired:
                case TicketTransferCode.ValidationInfoNotAvailable:
                case TicketTransferCode.TicketAmountDoesNotMatchSystem:
                case TicketTransferCode.TicketAmountExceedsAutoRedemptionLimit:
                case TicketTransferCode.TicketNotValidAtThisTime:
                case TicketTransferCode.TicketNotValidOnThisGamingMachine:
                case TicketTransferCode.PlayerCardMustBeInserted:
                case TicketTransferCode.TicketNotValidForCurrentPlayer:
                    _redemptionStatusCode = RedemptionStatusCode.TicketRejectedByHost;
                    break;
                case TicketTransferCode.ValidCashableTicket:
                case TicketTransferCode.ValidRestrictedPromotionalTicket:
                case TicketTransferCode.ValidNonRestrictedPromotionalTicket:
                case TicketTransferCode.RequestForCurrentTicketStatus:
                    isNonValidTicket = false;
                    break;
                default:
                    _redemptionStatusCode = RedemptionStatusCode.NotAValidTransferFunction;
                    break;
            }
            return isNonValidTicket;
        }

        private bool IsDuplicate71Command(RedeemTicketData data)
        {
            var ticketInInfo = _sasVoucherInProvider.CurrentTicketInfo;
            return (data.TransferCode == ticketInInfo.TransferCode) &&
                   (data.TransferAmount == ticketInInfo.Amount) &&
                   (data.ParsingCode == (int)ParsingCode.Bcd) &&
                   (data.Barcode == ticketInInfo.Barcode);
        }

        private RedeemTicketResponse CreateTicketResponse(RedemptionStatusCode code)
        {
            return new RedeemTicketResponse
            {
                MachineStatus = code,
                ParsingCode = ParsingCode.Bcd,
                TransferAmount = _sasVoucherInProvider.CurrentTicketInfo.Amount,
                Barcode = string.IsNullOrEmpty(_sasVoucherInProvider.CurrentTicketInfo.Barcode) ?
                NilBarcode : _sasVoucherInProvider.CurrentTicketInfo.Barcode,
                Handlers = _nullHandler
            };
        }
    }
}
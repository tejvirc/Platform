namespace Aristocrat.Monaco.Sas.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client;

    /// <inheritdoc />
    public class SasExceptionHandler : ISasExceptionHandler
    {
        private static readonly IReadOnlyDictionary<GeneralExceptionCode, SasGroup> ExceptionMapping =
            new Dictionary<GeneralExceptionCode, SasGroup>
            {
                { GeneralExceptionCode.EgmPowerApplied, SasGroup.PerClientLoad },
                { GeneralExceptionCode.EgmPowerLost, SasGroup.PerClientLoad },
                { GeneralExceptionCode.AftRegistrationAcknowledged, SasGroup.Aft },
                { GeneralExceptionCode.AftRegistrationCanceled, SasGroup.Aft },
                { GeneralExceptionCode.AftRequestForHostCashOut, SasGroup.Aft },
                { GeneralExceptionCode.AftRequestForHostToCashOutWin, SasGroup.Aft },
                { GeneralExceptionCode.AftRequestToRegister, SasGroup.Aft },
                { GeneralExceptionCode.AftTransferComplete, SasGroup.Aft },
                { GeneralExceptionCode.AttendantMenuEntered, SasGroup.PerClientLoad },
                { GeneralExceptionCode.AttendantMenuExited, SasGroup.PerClientLoad },
                { GeneralExceptionCode.AuthenticationComplete, SasGroup.GeneralControl },
                { GeneralExceptionCode.BellyDoorWasClosed, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BellyDoorWasOpened, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAccepted, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAccepted1, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAccepted2, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAccepted5, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAccepted10, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAccepted20, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAccepted50, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAccepted100, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAccepted200, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAccepted500, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAcceptorHardwareFailure, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillAcceptorVersionChanged, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillJam, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillRejected, SasGroup.PerClientLoad },
                { GeneralExceptionCode.BillValidatorPeriodMetersReset, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CardCageWasClosed, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CardCageWasOpened, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CardHeldOrNotHeld, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CashBoxDoorWasClosed, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CashBoxDoorWasOpened, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CashBoxFullDetected, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CashBoxNearFullDetected, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CashBoxWasInstalled, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CashBoxWasRemoved, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CashOutButtonPressed, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CashOutTicketPrinted, SasGroup.Validation },
                { GeneralExceptionCode.ChangeLampOff, SasGroup.PerClientLoad },
                { GeneralExceptionCode.ChangeLampOn, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CoinInLockoutMalfunction, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CoinInTilt, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CoinOutTilt, SasGroup.PerClientLoad },
                { GeneralExceptionCode.ComponentListChanged, SasGroup.GeneralControl },
                { GeneralExceptionCode.CounterfeitBillDetected, SasGroup.PerClientLoad },
                { GeneralExceptionCode.CreditWagered, SasGroup.PerClientLoad },
                { GeneralExceptionCode.DiverterMalfunction, SasGroup.PerClientLoad },
                { GeneralExceptionCode.DropDoorWasClosed, SasGroup.PerClientLoad },
                { GeneralExceptionCode.DropDoorWasOpened, SasGroup.PerClientLoad },
                { GeneralExceptionCode.EePromBadDeviceError, SasGroup.PerClientLoad },
                { GeneralExceptionCode.EePromDataError, SasGroup.PerClientLoad },
                { GeneralExceptionCode.EePromErrorBadChecksum, SasGroup.PerClientLoad },
                { GeneralExceptionCode.EePromErrorDifferentChecksum, SasGroup.PerClientLoad },
                { GeneralExceptionCode.EnabledGamesDenomsChanged, SasGroup.PerClientLoad },
                { GeneralExceptionCode.ExtraCoinPaid, SasGroup.PerClientLoad },
                { GeneralExceptionCode.GameHasEnded, SasGroup.GameStartEnd },
                { GeneralExceptionCode.GameHasStarted, SasGroup.GameStartEnd },
                { GeneralExceptionCode.GameLocked, SasGroup.Aft },
                { GeneralExceptionCode.GameRecallEntryHasBeenDisplayed, SasGroup.PerClientLoad },
                { GeneralExceptionCode.GameSelected, SasGroup.PerClientLoad },
                { GeneralExceptionCode.GamingMachineOutOfServiceByOperator, SasGroup.PerClientLoad },
                { GeneralExceptionCode.GamingMachineSoftMetersReset, SasGroup.PerClientLoad },
                { GeneralExceptionCode.GeneralTilt, SasGroup.PerClientLoad },
                { GeneralExceptionCode.HandPayIsPending, SasGroup.PerClientLoad },
                { GeneralExceptionCode.HandPayValidated, SasGroup.Validation },
                { GeneralExceptionCode.HandPayWasReset, SasGroup.PerClientLoad },
                { GeneralExceptionCode.HopperEmptyDetected, SasGroup.PerClientLoad },
                { GeneralExceptionCode.HopperFullDetected, SasGroup.PerClientLoad },
                { GeneralExceptionCode.HopperLevelLowDetected, SasGroup.PerClientLoad },
                { GeneralExceptionCode.JackpotHandpayKeyedOffToMachinePay, SasGroup.Validation },
                { GeneralExceptionCode.LegacyBonusPayAwarded, SasGroup.LegacyBonus },
                { GeneralExceptionCode.LowBackupBatteryDetected, SasGroup.PerClientLoad },
                { GeneralExceptionCode.MemoryErrorReset, SasGroup.PerClientLoad },
                { GeneralExceptionCode.MeterChangeCanceled, SasGroup.PerClientLoad },
                { GeneralExceptionCode.MeterChangePending, SasGroup.PerClientLoad },
                { GeneralExceptionCode.NoProgressiveInformationHasBeenReceivedFor5Seconds, SasGroup.Progressives },
                { GeneralExceptionCode.NvRamErrorBadDevice, SasGroup.PerClientLoad },
                { GeneralExceptionCode.NvRamErrorDataRecovered, SasGroup.PerClientLoad },
                { GeneralExceptionCode.NvRamErrorNoDataRecovered, SasGroup.PerClientLoad },
                { GeneralExceptionCode.OperatorChangedOptions, SasGroup.PerClientLoad },
                { GeneralExceptionCode.OperatorMenuEntered, SasGroup.PerClientLoad },
                { GeneralExceptionCode.OperatorMenuExited, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PartitionedEPromErrorBadChecksum, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PartitionedEPromErrorVersionChanged, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PlayerCanceledTheHandPayRequest, SasGroup.Validation },
                { GeneralExceptionCode.PlayerHasRequestedDrawCards, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PowerOffCardCageAccess, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PowerOffCashBoxDoorAccess, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PowerOffDropDoorAccess, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PowerOffSlotDoorAccess, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PrinterCarriageJam, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PrinterCommunicationError, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PrinterPaperLow, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PrinterPaperOutError, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PrinterPowerOff, SasGroup.PerClientLoad },
                { GeneralExceptionCode.PrinterPowerOn, SasGroup.PerClientLoad },
                { GeneralExceptionCode.ProgressiveWin, SasGroup.Progressives },
                { GeneralExceptionCode.Reel1Tilt, SasGroup.PerClientLoad },
                { GeneralExceptionCode.Reel2Tilt, SasGroup.PerClientLoad },
                { GeneralExceptionCode.Reel3Tilt, SasGroup.PerClientLoad },
                { GeneralExceptionCode.Reel4Tilt, SasGroup.PerClientLoad },
                { GeneralExceptionCode.Reel5Tilt, SasGroup.PerClientLoad },
                { GeneralExceptionCode.ReelTilt, SasGroup.PerClientLoad },
                { GeneralExceptionCode.ReelMechanismDisconnected, SasGroup.PerClientLoad },
                { GeneralExceptionCode.ReelNHasStopped, SasGroup.PerClientLoad },
                { GeneralExceptionCode.ReplacePrinterRibbon, SasGroup.PerClientLoad },
                { GeneralExceptionCode.ReverseBillDetected, SasGroup.PerClientLoad },
                { GeneralExceptionCode.SasProgressiveLevelHit, SasGroup.Progressives },
                { GeneralExceptionCode.SessionEnd, SasGroup.PerClientLoad },
                { GeneralExceptionCode.SessionStart, SasGroup.PerClientLoad },
                { GeneralExceptionCode.SlotDoorWasClosed, SasGroup.PerClientLoad },
                { GeneralExceptionCode.SlotDoorWasOpened, SasGroup.PerClientLoad },
                { GeneralExceptionCode.SystemValidationRequest, SasGroup.Validation },
                { GeneralExceptionCode.TicketHasBeenInserted, SasGroup.Validation },
                { GeneralExceptionCode.TicketTransferComplete, SasGroup.Validation },
                { GeneralExceptionCode.TipAwarded, SasGroup.PerClientLoad },
                { GeneralExceptionCode.UserInactivity, SasGroup.PerClientLoad },
                { GeneralExceptionCode.ValidationIdNotConfigured, SasGroup.Validation },
                { GeneralExceptionCode.NonSasProgressiveLevelHit, SasGroup.PerClientLoad }
            };

        private readonly object _lockObject = new object();
        private readonly IDictionary<SasGroup, IList<ISasExceptionQueue>> _exceptionProcessors = new Dictionary<SasGroup, IList<ISasExceptionQueue>>();

        /// <inheritdoc />
        public void RegisterExceptionProcessor(SasGroup group, ISasExceptionQueue exceptionQueue)
        {
            lock (_lockObject)
            {
                if (_exceptionProcessors.ContainsKey(group))
                {
                    _exceptionProcessors[group].Add(exceptionQueue);
                }
                else
                {
                    _exceptionProcessors.Add(group, new List<ISasExceptionQueue> { exceptionQueue });
                }
            }
        }

        /// <inheritdoc />
        public void RemoveExceptionQueue(SasGroup group, ISasExceptionQueue exceptionQueue)
        {
            lock (_lockObject)
            {
                if (_exceptionProcessors.ContainsKey(group))
                {
                    _exceptionProcessors[group].Remove(exceptionQueue);
                }
            }
        }

        /// <inheritdoc />
        public void ReportException(ISasExceptionCollection exception) =>
            ReportException(exception, GetExceptionQueues(exception.ExceptionCode));

        /// <inheritdoc />
        public void ReportException(ISasExceptionCollection exception, byte clientNumber) =>
            ReportException(exception, GetExceptionQueues(exception.ExceptionCode, clientNumber));

        /// <inheritdoc />
        public void RemoveException(ISasExceptionCollection exception) =>
            RemoveException(exception, GetExceptionQueues(exception.ExceptionCode));

        /// <inheritdoc />
        public void RemoveException(ISasExceptionCollection exception, byte clientNumber) =>
            RemoveException(exception, GetExceptionQueues(exception.ExceptionCode, clientNumber));

        /// <inheritdoc />
        public void ReportException(Func<byte, ISasExceptionCollection> exceptionProvider, GeneralExceptionCode exceptionCode)
        {
            lock (_lockObject)
            {
                foreach (var processor in GetExceptionQueues(exceptionCode))
                {
                    var exception = exceptionProvider.Invoke(processor.ClientNumber);
                    if (exception?.ExceptionCode != exceptionCode)
                    {
                        continue;
                    }

                    processor.QueueException(exception);
                }
            }
        }

        /// <inheritdoc />
        public void AddHandler(GeneralExceptionCode code, Action action)
        {
            lock (_lockObject)
            {
                foreach (var processor in GetExceptionQueues(code))
                {
                    processor.AddHandler(code, action);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveHandler(GeneralExceptionCode code)
        {
            lock (_lockObject)
            {
                foreach (var processor in GetExceptionQueues(code))
                {
                    processor.RemoveHandler(code);
                }
            }
        }

        private void ReportException(ISasExceptionCollection exception, IEnumerable<ISasExceptionQueue> exceptionQueues)
        {
            lock (_lockObject)
            {
                foreach (var processor in exceptionQueues)
                {
                    processor.QueueException(exception);
                }
            }
        }

        private void RemoveException(ISasExceptionCollection exception, IEnumerable<ISasExceptionQueue> exceptionQueues)
        {
            lock (_lockObject)
            {
                foreach (var processor in exceptionQueues)
                {
                    processor.RemoveException(exception);
                }
            }
        }

        private IEnumerable<ISasExceptionQueue> GetExceptionQueues(GeneralExceptionCode code, byte? clientNumber = null)
        {
            lock (_lockObject)
            {
                if (!ExceptionMapping.TryGetValue(code, out var group) ||
                    !_exceptionProcessors.TryGetValue(group, out var processors))
                {
                    return new List<ISasExceptionQueue>();
                }

                return clientNumber.HasValue ? processors.Where(x => x.ClientNumber == clientNumber.Value) : processors;
            }
        }
    }
}
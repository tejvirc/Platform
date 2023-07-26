namespace Aristocrat.Monaco.Hardware.Pwm.CoinAcceptor
{
    using System;
    using System.Reflection;
    using System.Threading;
    using Contracts.Gds;
    using Contracts.PWM;
    using Protocol;
    using log4net;

    /// <summary>
    ///     Class to manage communication layer for coin acceptor devices
    ///     which are based on pulse width modulated signal <see cref="PwmDeviceProtocol"
    /// </summary>
    public abstract class CoinAcceptorCommunicator : PwmDeviceProtocol
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        protected readonly object Lock = new();
        protected CoinAcceptorState AcceptorState = new();
        protected CoinEntryState CoinEntryState = new();

        /// <inheritdoc/>
        public override void SendMessage(GdsSerializableMessage message, CancellationToken token)
        {
            ProcessMessage(message);
        }

        /// <summary>Reset the device</summary>
        protected void Reset()
        {
            lock (Lock)
            {
                Logger.Info($"{Manufacturer}: Coin acceptor Resetting");
                StopPolling();
                CoinEntryState.Reset();
                AcceptorState.Reset();
                StartPolling();
            }
        }

        /// <summary>Ack Read of device</summary>
        /// <param name="txnId">transaction id</param>
        protected virtual bool AckRead(uint txnId)
        {
            //Non Non-Volatile Coin acceptor has nothing do with ACK
            return true;
        }

        /// <summary>Enable/Disable Reject Mechanism</summary>
        /// <param name="OnOff">Flag to enable/disable</param>
        protected bool RejectMechanishOnOff(bool OnOff)
        {
            return Ioctl(OnOff ?
                CoinAcceptorCommands.CoinAcceptorRejectOn
                : CoinAcceptorCommands.CoinAcceptorRejectOff, 0);
        }

        /// <summary>Enable/Disable the Diverter </summary>
        /// <param name="OnOff">Flag to enable/disable</param>
        protected bool DivertorMechanishOnOff(bool OnOff)
        {
            return Ioctl(OnOff ?
                CoinAcceptorCommands.CoinAcceptorDivertorOn
                : CoinAcceptorCommands.CoinAcceptorDivertorOff, 0);
        }

        /// <summary>Process a GDS message into protocol calls.</summary>
        /// <param name="message">GDS message</param>
        protected void ProcessMessage(GdsSerializableMessage message)
        {
            switch (message)
            {
                case DivertorMode mode:
                    {
                        if (mode.DivertorOnOff)
                        {
                            DivertToHopper();
                        }
                        else
                        {
                            DivertToCashbox();
                        }
                        break;
                    }
                case RejectMode mode:
                    {
                        if (mode.RejectOnOff)
                        {
                            CoinRejectMechOff();
                        }
                        else
                        {
                            CoinRejectMechOn();
                        }

                        break;
                    }
                case DeviceReset:
                    {
                        Reset();
                        break;
                    }
                default:
                    break;
            }
        }

        /// <summary>Enable reject mechanism</summary>
        protected void CoinRejectMechOn()
        {
            lock (Lock)
            {
                if (AcceptorState.PendingDiverterAction == DivertorAction.None)
                {
                    RejectMechanishOnOff(true);
                }

                AcceptorState.State = Contracts.PWM.AcceptorState.Reject;
            }
        }

        /// <summary>Run method for thread</summary>
        protected override void Run()
        {
            while (Running)
            {
                Poll.WaitOne();
                var (status, record) = Read();

                if (status)
                {
                    lock (Lock)
                    {
                        if (AcceptorState.CoinTransmitTimer < CoinSignalsConsts.CoinTransitTime)
                        {
                            AcceptorState.CoinTransmitTimer += record.elapsedSinceLastChange.QuadPart / 10000;
                        }

                        if (AcceptorState.State == Contracts.PWM.AcceptorState.Accept || AcceptorState.CoinTransmitTimer < CoinSignalsConsts.CoinTransitTime)
                        {
                            DataProcessor(record);
                        }

                        AckRead(record.changeId);

                        if ((AcceptorState.PendingDiverterAction != DivertorAction.None)
                            && AcceptorState.CoinTransmitTimer >= CoinSignalsConsts.CoinTransitTime)
                        {
                            if (AcceptorState.PendingDiverterAction == DivertorAction.DivertToHopper)
                            {
                                DivertorMechanishOnOff(true);
                                AcceptorState.DivertTo = DivertorState.DivertToHopper;
                            }
                            else
                            {
                                DivertorMechanishOnOff(false);
                                AcceptorState.DivertTo = DivertorState.DivertToCashbox;
                            }

                            AcceptorState.PendingDiverterAction = DivertorAction.None;
                            RejectMechanishOnOff(AcceptorState.State != Contracts.PWM.AcceptorState.Accept);
                        }
                    }
                }
            }
        }

        /// <summary>Process data</summary>
        private void DataProcessor(ChangeRecord record)
        {
            CoinEntryState.currentState = Cc62Signals.None;
            ProcessDiverterSignal(record);
            ProcessSenseSignal(record);
            ProcessCreditSignal(record);
            ProcessAlarmSignal(record);
        }

        /// <summary>Process diverter signal</summary>
        private void ProcessDiverterSignal(ChangeRecord record)
        {
            if (CoinEntryState.currentState != Cc62Signals.None)
            {
                //Panic
                throw new InvalidOperationException();
            }

            CoinEntryState.currentState = Cc62Signals.SolenoidSignal;
            CoinEntryState.DivertingTo = ((record.newValue & (int)Cc62Signals.SolenoidSignal) != 0)
                                            ? DivertorState.DivertToCashbox
                                            : DivertorState.DivertToHopper;
        }

        /// <summary>Process sense signal</summary>
        private void ProcessSenseSignal(ChangeRecord record)
        {
            if (CoinEntryState.currentState != Cc62Signals.SolenoidSignal)
            {
                //Panic
                throw new InvalidOperationException();
            }

            CoinEntryState.currentState = Cc62Signals.SenseSignal;
            switch (CoinEntryState.SenseState)
            {
                case SenseSignalState.HighToLow:
                    if ((record.newValue & (int)Cc62Signals.SenseSignal) == 0)
                    {
                        CoinEntryState.SenseState = SenseSignalState.LowToHigh;
                        CoinEntryState.SenseTime = 0;
                        break;
                    }
                    break;
                case SenseSignalState.LowToHigh:
                    CoinEntryState.SenseTime += record.elapsedSinceLastChange.QuadPart / 10000;
                    if (CoinEntryState.SenseTime > CoinSignalsConsts.SensePulseMax)
                    {
                        CoinEntryState.SenseState = SenseSignalState.Fault;
                        OnMessageReceived(new CoinInFaultStatus { FaultType = CoinFaultTypes.Optic });
                        break;
                    }

                    if ((record.newValue & (int)Cc62Signals.SenseSignal) != 0)
                    {
                        CoinEntryState.SenseState = SenseSignalState.HighToLow;
                        if (CoinEntryState.SenseTime < CoinSignalsConsts.SensePulseMin)
                        {
                            //Noise
                            break;
                        }
                        CoinEntryState.SensePulses++;
                        CoinEntryState.SenseToCreditTime = 0;
                    }
                    break;
                case SenseSignalState.Fault:
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>Process credit signal</summary>
        private void ProcessCreditSignal(ChangeRecord record)
        {
            if (CoinEntryState.currentState != Cc62Signals.SenseSignal)
            {
                //Panic
                throw new InvalidOperationException();
            }

            CoinEntryState.currentState = Cc62Signals.CreditSignal;
            switch (CoinEntryState.CreditState)
            {
                case CreditSignalState.HighToLow:
                    CheckCreditToSensePulse(record.elapsedSinceLastChange.QuadPart / 10000);
                    if ((record.newValue & (int)Cc62Signals.CreditSignal) == 0)
                    {
                        CoinEntryState.CreditState = CreditSignalState.LowToHigh;
                        CoinEntryState.CreditTime = 0;
                        break;
                    }

                    break;
                case CreditSignalState.LowToHigh:
                    CheckCreditToSensePulse(record.elapsedSinceLastChange.QuadPart / 10000);
                    CoinEntryState.CreditTime += record.elapsedSinceLastChange.QuadPart / 10000;
                    if (CoinEntryState.CreditTime > CoinSignalsConsts.CreditPulseMax)
                    {
                        CoinEntryState.CreditState = CreditSignalState.Fault;
                        OnMessageReceived(new CoinInFaultStatus { FaultType = CoinFaultTypes.Optic });
                        break;
                    }

                    if ((record.newValue & (int)Cc62Signals.CreditSignal) != 0)
                    {
                        CoinEntryState.CreditState = CreditSignalState.HighToLow;
                        if (CoinEntryState.CreditTime < CoinSignalsConsts.CreditPulseMin)
                        {
                            //Noise
                            break;
                        }

                        if (CoinEntryState.SensePulses > 0)
                        {
                            CoinEntryState.SensePulses--;
                            System.Console.WriteLine("Firing " + nameof(CoinInEvent));
                            OnMessageReceived(new CoinInStatus { EventType = CoinEventType.CoinInEvent });
                            if (CoinEntryState.DivertingTo == DivertorState.DivertToHopper)
                            {
                                if (AcceptorState.DivertTo == DivertorState.DivertToHopper)
                                {
                                    OnMessageReceived(new CoinInStatus { EventType = CoinEventType.CoinToHopperInEvent });
                                }
                                else
                                {
                                    OnMessageReceived(new CoinInStatus { EventType = CoinEventType.CoinToHopperInsteadOfCashboxEvent });
                                }
                            }
                            else
                            {
                                if (CoinEntryState.DivertingTo == DivertorState.DivertToCashbox)
                                {
                                    if (AcceptorState.DivertTo == DivertorState.DivertToCashbox)
                                    {
                                        OnMessageReceived(new CoinInStatus { EventType = CoinEventType.CoinToCashboxInEvent });

                                    }
                                    else
                                    {
                                        OnMessageReceived(new CoinInStatus { EventType = CoinEventType.CoinToCashboxInsteadOfHopperEvent });
                                    }
                                }
                            }
                            break;
                        }
                        OnMessageReceived(new CoinInFaultStatus { FaultType = CoinFaultTypes.Invalid });
                        break;
                    }

                    break;
                case CreditSignalState.Fault:
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>Process alarm signal</summary>
        private void ProcessAlarmSignal(ChangeRecord record)
        {
            if (CoinEntryState.currentState != Cc62Signals.CreditSignal)
            {
                //Panic
                throw new InvalidOperationException();
            }

            CoinEntryState.currentState = Cc62Signals.AlarmSignal;
            switch (CoinEntryState.AlarmState)
            {
                case AlarmSignalState.HighToLow:
                    if ((record.newValue & (int)Cc62Signals.AlarmSignal) == 0)
                    {
                        CoinEntryState.AlarmState = AlarmSignalState.LowToHigh;
                        CoinEntryState.AlarmTime = 0;
                        break;
                    }
                    break;
                case AlarmSignalState.LowToHigh:

                    CoinEntryState.AlarmTime += record.elapsedSinceLastChange.QuadPart / 10000;
                    if (CoinEntryState.AlarmTime > CoinSignalsConsts.AlarmPulseMax)
                    {
                        CoinEntryState.AlarmState = AlarmSignalState.Fault;
                        OnMessageReceived(new CoinInFaultStatus { FaultType = CoinFaultTypes.Optic });
                        break;
                    }

                    if ((record.newValue & (int)Cc62Signals.AlarmSignal) != 0)
                    {
                        CoinEntryState.AlarmState = AlarmSignalState.HighToLow;
                        if (CoinEntryState.AlarmTime < CoinSignalsConsts.AlarmPulseMin)
                        {
                            //Noise
                            break;
                        }
                        OnMessageReceived(new CoinInFaultStatus { FaultType = CoinFaultTypes.YoYo });
                        break;
                    }
                    break;
                case AlarmSignalState.Fault:
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>Check credit to sense pulse</summary>
        private void CheckCreditToSensePulse(Int64 time)
        {
            CoinEntryState.SenseToCreditTime += time;
            if (CoinEntryState.SenseToCreditTime > CoinSignalsConsts.SenseToCreditMaxtime)
            {
                CoinEntryState.SensePulses = 0;
            }
        }

        /// <summary>Disable reject mechanism</summary>
        private void CoinRejectMechOff()
        {
            lock (Lock)
            {
                if (AcceptorState.PendingDiverterAction == DivertorAction.None)
                {
                    RejectMechanishOnOff(false);
                }
                AcceptorState.State = Contracts.PWM.AcceptorState.Accept;
            }
        }

        /// <summary>Divert diverter toward hopper</summary>
        private void DivertToHopper()
        {
            lock (Lock)
            {
                if (AcceptorState.DivertTo == DivertorState.DivertToCashbox)
                {
                    RejectMechanishOnOff(true);
                    AcceptorState.PendingDiverterAction = DivertorAction.DivertToHopper;
                    AcceptorState.CoinTransmitTimer = 0;

                }
            }
        }

        /// <summary>Divert diverter toward cashbox</summary>
        private void DivertToCashbox()
        {
            lock (Lock)
            {
                if (AcceptorState.DivertTo == DivertorState.DivertToHopper)
                {
                    RejectMechanishOnOff(true);
                    AcceptorState.PendingDiverterAction = DivertorAction.DivertToCashbox;
                    AcceptorState.CoinTransmitTimer = 0;

                }
            }
        }

        /// <summary>Start polling on device</summary>
        private void StartPolling()
        {
            _ = Ioctl(CoinAcceptorCommands.CoinAcceptorStartPolling, 0);
        }

        /// <summary>Stop polling on device</summary>
        private void StopPolling()
        {
            _ = Ioctl(CoinAcceptorCommands.CoinAcceptorStopPolling, 0);
        }
    }
}

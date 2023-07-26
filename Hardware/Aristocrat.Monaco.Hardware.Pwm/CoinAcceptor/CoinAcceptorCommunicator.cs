namespace Aristocrat.Monaco.Hardware.Pwm.CoinAcceptor
{
    using Aristocrat.Monaco.Hardware.Contracts.Communicator;
    using Aristocrat.Monaco.Hardware.Contracts.Gds;
    using Aristocrat.Monaco.Hardware.Contracts.PWM;
    using Aristocrat.Monaco.Hardware.Pwm.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public abstract class CoinAcceptorCommunicator : PwmDeviceProtocol
    {
        protected readonly object Lock = new();
        protected CoinAcceptorState AcceptorState = new();
        protected CoinEntryState CoinEntryState = new();



        protected override void Run()
        {
            while (_running)
            {
                Poll.WaitOne();
                (var status, var record) = Read();

                if (status)
                {
                    lock (Lock)
                    {
                        // System.Console.WriteLine("Pawan Value = " + record.newValue + " Time = " + record.elapsedSinceLastChange.QuadPart / 10000);
                        //  System.Console.WriteLine("Firing " + new CoinRawDataEvent().ToString());
                        // As of now no need for CoinRawDataEvent
                        //_bus.Publish(new CoinRawDataEvent());

                        if (AcceptorState.CoinTransmitTimer < CoinSignalsConsts.CoinTransitTime)
                        {
                            AcceptorState.CoinTransmitTimer += record.elapsedSinceLastChange.QuadPart / 10000;
                        }

                        if (AcceptorState.State == Contracts.PWM.AcceptorState.Accept || AcceptorState.CoinTransmitTimer < CoinSignalsConsts.CoinTransitTime)
                        {
                            DataProcessor(record);
                        }
                        AckRead(record.changeId);

                        if ((AcceptorState.pendingDiverterAction != DivertorAction.None)
                            && AcceptorState.CoinTransmitTimer >= CoinSignalsConsts.CoinTransitTime)
                        {
                            if (AcceptorState.pendingDiverterAction == DivertorAction.DivertToHopper)
                            {
                                DivertorMechanishOnOff(true);
                                AcceptorState.DivertTo = DivertorState.DivertToHopper;
                            }
                            else
                            {
                                DivertorMechanishOnOff(false);
                                AcceptorState.DivertTo = DivertorState.DivertToCashbox;
                            }

                            AcceptorState.pendingDiverterAction = DivertorAction.None;
                            RejectMechanishOnOff(AcceptorState.State != Contracts.PWM.AcceptorState.Accept);
                        }
                    }
                }
            }
        }

        private void DataProcessor(Contracts.PWM.ChangeRecord record)
        {
            CoinEntryState.currentState = Cc62Signals.None;

            ProcessDiverterSignal(record);
            ProcessSenseSignal(record);
            ProcessCreditSignal(record);
            ProcessAlarmSignal(record);
        }

        private void ProcessDiverterSignal(ChangeRecord record)
        {
            if (CoinEntryState.currentState != Cc62Signals.None)
            {
                //Panic
                throw new InvalidOperationException();
            }
            CoinEntryState.currentState = Cc62Signals.SolenoidSignal;

            CoinEntryState.DivertingTo = (((int)record.newValue & (int)Cc62Signals.SolenoidSignal) != 0)
                                            ? DivertorState.DivertToCashbox
                                            : DivertorState.DivertToHopper;
        }

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
                    if (((int)record.newValue & (int)Cc62Signals.SenseSignal) == 0)
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
                        System.Console.WriteLine("Firing Optic");
                        OnMessageReceived(new CoinInFaultStatus { FaultType = CoinFaultTypes.Optic });
                        //TBD _bus.Publish(new HardwareFaultEvent(CoinFaultTypes.Optic));
                        break;
                    }

                    if (((int)record.newValue & (int)Cc62Signals.SenseSignal) != 0)
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
                    if (((int)record.newValue & (int)Cc62Signals.CreditSignal) == 0)
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
                        System.Console.WriteLine("Firing optic");
                        //TBD _bus.Publish(new HardwareFaultEvent(CoinFaultTypes.Optic));
                        OnMessageReceived(new CoinInFaultStatus { FaultType = CoinFaultTypes.Optic });
                        break;
                    }

                    if (((int)record.newValue & (int)Cc62Signals.CreditSignal) != 0)
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
                            //TBD _bus.Publish(new CoinInEvent(new Coin() { Value = _tokenValue }));
                            if (CoinEntryState.DivertingTo == DivertorState.DivertToHopper)
                            {
                                if (AcceptorState.DivertTo == DivertorState.DivertToHopper)
                                {
                                    System.Console.WriteLine("Firing " + new CoinToHopperInEvent().ToString());
                                    //TBD _bus.Publish(new CoinToHopperInEvent());
                                    OnMessageReceived(new CoinInStatus { EventType = CoinEventType.CoinToHopperInEvent });
                                }
                                else
                                {
                                    System.Console.WriteLine("Firing " + new CoinToHopperInsteadOfCashboxEvent().ToString());
                                    //TBD _bus.Publish(new CoinToHopperInsteadOfCashboxEvent());
                                    OnMessageReceived(new CoinInStatus { EventType = CoinEventType.CoinToHopperInsteadOfCashboxEvent });
                                }
                            }
                            else
                            {
                                if (CoinEntryState.DivertingTo == DivertorState.DivertToCashbox)
                                {
                                    if (AcceptorState.DivertTo == DivertorState.DivertToCashbox)
                                    {
                                        System.Console.WriteLine("Firing " + new CoinToCashboxInEvent().ToString());
                                        //TBD _bus.Publish(new CoinToCashboxInEvent());
                                        OnMessageReceived(new CoinInStatus { EventType = CoinEventType.CoinToCashboxInEvent });

                                    }
                                    else
                                    {
                                        System.Console.WriteLine("Firing " + new CoinToCashboxInsteadOfHopperEvent().ToString());
                                        //TBD _bus.Publish(new CoinToCashboxInsteadOfHopperEvent());
                                        OnMessageReceived(new CoinInStatus { EventType = CoinEventType.CoinToCashboxInsteadOfHopperEvent });
                                    }
                                }
                            }
                            break;
                        }
                        System.Console.WriteLine("Firing invalid");
                        //TBD_bus.Publish(new HardwareFaultEvent(CoinFaultTypes.Invalid));
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
                    if (((int)record.newValue & (int)Cc62Signals.AlarmSignal) == 0)
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
                        System.Console.WriteLine("Firing optic");
                        //TBD _bus.Publish(new HardwareFaultEvent(CoinFaultTypes.Optic));
                        OnMessageReceived(new CoinInFaultStatus { FaultType = CoinFaultTypes.Optic });

                        break;

                    }

                    if (((int)record.newValue & (int)Cc62Signals.AlarmSignal) != 0)
                    {
                        CoinEntryState.AlarmState = AlarmSignalState.HighToLow;
                        if (CoinEntryState.AlarmTime < CoinSignalsConsts.AlarmPulseMin)
                        {
                            //Noise
                            break;
                        }
                        System.Console.WriteLine("Firing yoyo");
                        //TBD_bus.Publish(new HardwareFaultEvent(CoinFaultTypes.YoYo));
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

        private void CheckCreditToSensePulse(Int64 time)
        {
            CoinEntryState.SenseToCreditTime += time;
            if (CoinEntryState.SenseToCreditTime > CoinSignalsConsts.SenseToCreditMaxtime)
            {
                CoinEntryState.SensePulses = 0;
            }
        }

        protected bool CoinRejectMechOn()
        {
            lock (Lock)
            {
                if (AcceptorState.pendingDiverterAction == DivertorAction.None)
                {
                    RejectMechanishOnOff(true);
                }

                AcceptorState.State = Contracts.PWM.AcceptorState.Reject;
                return true;
            }
        }

        private bool CoinRejectMechOff()
        {
            lock (Lock)
            {
                if (AcceptorState.pendingDiverterAction == DivertorAction.None)
                {
                    RejectMechanishOnOff(false);
                }
                AcceptorState.State = Contracts.PWM.AcceptorState.Accept;
                return true;
            }
        }


        private bool DivertToHopper()
        {
            lock (Lock)
            {
                if (AcceptorState.DivertTo == DivertorState.DivertToCashbox)
                {
                    RejectMechanishOnOff(true);
                    AcceptorState.pendingDiverterAction = DivertorAction.DivertToHopper;
                    AcceptorState.CoinTransmitTimer = 0;

                }
                return true;
            }
        }

        private bool DivertToCashbox()
        {
            lock (Lock)
            {
                if (AcceptorState.DivertTo == DivertorState.DivertToHopper)
                {
                    RejectMechanishOnOff(true);
                    AcceptorState.pendingDiverterAction = DivertorAction.DivertToCashbox;
                    AcceptorState.CoinTransmitTimer = 0;

                }
                return true;
            }
        }

        protected void Reset()
        {
            lock (Lock)
            {
                System.Console.WriteLine("Clearing lockup");
                StopPolling();
                CoinEntryState.Reset();
                AcceptorState.Reset();
                StartPolling();
            }
        }

        protected virtual bool AckRead(uint txnId)
        {
            //Non Non-Volatile Coin acceptor has nothing do with ACK
            return true;
        }

        protected bool RejectMechanishOnOff(bool OnOff)
        {
            return Ioctl(OnOff ?
                CoinAcceptorCommands.CoinAcceptorRejectOn
                : CoinAcceptorCommands.CoinAcceptorRejectOff, 0);
        }
        protected bool DivertorMechanishOnOff(bool OnOff)
        {
            return Ioctl(OnOff ?
                CoinAcceptorCommands.CoinAcceptorDivertorOn
                : CoinAcceptorCommands.CoinAcceptorDivertorOff, 0);
        }

        private bool StartPolling()
        {
            return Ioctl(CoinAcceptorCommands.CoinAcceptorStartPolling, 0);
        }

        private bool StopPolling()
        {
            return Ioctl(CoinAcceptorCommands.CoinAcceptorStopPolling, 0);
        }


        public override void SendMessage(GdsSerializableMessage message, CancellationToken token)
        {
            ProcessMessage(message, token);
        }

        protected void ProcessMessage(GdsSerializableMessage message, CancellationToken token)
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

    }
}

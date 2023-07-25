namespace Aristocrat.Monaco.Hardware.Pwm.CoinAcceptor
{
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Communicator;
    using Aristocrat.Monaco.Hardware.Contracts.Gds;
    using Aristocrat.Monaco.Hardware.Contracts.PWM;
    using Aristocrat.Monaco.Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public abstract class CoinAcceptor : PwmCommunicator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected readonly object Lock = new();
        protected CoinAcceptorState AcceptorState = new();
        protected CoinEntryState CoinEntryState = new();
        protected readonly IPropertiesManager PropertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

        public override bool Configure(IComConfiguration comConfiguration)
        {
            DeviceConfig = new PwmDeviceConfig
            {
                DeviceInterface = new Guid("{e72a476b-664e-4a6b-9439-aa8cfa294ff2}"),
                Mode = CreateFileOption.Overlapped,
                pollingFrequency = 20,//ms
                waitPeriod = 20,//ms
                DeviceType = NativeConstants.CoinAcceptorDeviceType
            };
            Model = "CC-62";
            Manufacturer = "CC-62";

            return true;
        }

        protected void ProcessMessage(GdsSerializableMessage message, CancellationToken token)
        {
            switch (message)
            {
                case DivertorMode mode:
                    {
                        if(mode.DivertorOnOff)
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
                        if(mode.RejectOnOff)
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

        protected void DivertMechanismOnOff()
        {
            //TODO: implement hopper's properties with realtime values once hopper feature is available..
            bool isHopperInstalled = true;
            bool isHopperFull = false;

            if (PropertiesManager.GetValue(HardwareConstants.HopperEnabledKey, false) && isHopperInstalled && (!isHopperFull))
            {
                DivertToHopper();
            }
            else
            {
                DivertToCashbox();
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
                DivertMechanismOnOff();
            }
        }

        protected bool AckRead(uint txnId)
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
    }
}

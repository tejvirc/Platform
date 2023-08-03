namespace Aristocrat.Monaco.Hardware.Pwm.Hopper
{
    using System.Threading;
    using Contracts.CoinAcceptor;
    using Contracts.Gds;
    using Contracts.Gds.CoinAcceptor;
    using Contracts.Gds.Hopper;
    using Contracts.Hopper;
    using Protocol;


    /// <summary>
    ///     Class to manage communication layer for hopper devices
    ///     which are based on pulse width modulated signal <see cref="PwmDeviceProtocol"
    /// </summary>
    public abstract class HopperCommunicator: PwmDeviceProtocol
    {
        private LARGE_INTEGER _testDisconnectTimer = new LARGE_INTEGER();
        private int _disconnectCount = 0;
        private bool _disconnected = false;
        private int _totalPauseCount = 0;
        private int _maxCoinoutAllowed = 0;
        private int _currentCoinout = 0;
        protected readonly object Lock = new();
        private readonly CoinOutState _coinOutState = new CoinOutState();
        private readonly HopperState _hopperState = new HopperState();


        /// <summary>Defines the hopper type</summary>
        public abstract HopperType Type { get; }

        /// <inheritdoc/>
        public override void SendMessage(GdsSerializableMessage message, CancellationToken token)
        {
            ProcessMessage(message);
        }

         /// <summary>Run method for thread</summary>
        protected override void Run()
        {
            while (Running)
            {
                Poll.WaitOne();
                var(status, connected,  record) = ReadwithDisconnetTest();

                if (status)
                {
                    lock (Lock)
                    {
                        DataProcessor(record, connected);
                    }
                }
            }
        }

        private void DataProcessor(ChangeRecord record, bool connected)
        {
            _coinOutState.Timer -= (record.ElapsedSinceLastChange.QuadPart / 10000);

            if (_coinOutState.Timer < 0)
            {
                _coinOutState.Timer = 0;
            }

            switch (_coinOutState.State)
            {
                case HopperTaskState.WaitingForLeadingEdge:
                    ProcessWaitingForLeadingEdge(record, connected);
                    break;
                case HopperTaskState.WaitingForTrailingEdge:
                    ProcessWaitingForTrailingEdge(record, connected);
                    break;
                case HopperTaskState.WaitingForReset:
                    ProcessWaitingForReset(record, connected);
                    break;
                case HopperTaskState.WaitingForTimeout:
                    ProcessWaitingForTimeout(record, connected);
                    break;
                default:
                    break;
            }
        }


        private void ProcessWaitingForLeadingEdge(ChangeRecord record, bool connected)
        {
            if (CheckForDisconnect(connected))
            {
                return;
            }

            if (((int)record.NewValue & (int)HopperMasks.HopperCoinOutMask) != 0)
            {
                _coinOutState.State = HopperTaskState.WaitingForTrailingEdge;
                _coinOutState.Timer = HopperConsts.HopperMaxBlockedTime;
                if ((_hopperState.State & HopperMasks.HopperMotorDriveMask) == 0)
                {
                    StopHopperMotor();
                }
                return;
            }

            if ((_hopperState.State & HopperMasks.HopperMotorDriveMask) != 0)
            {
                if (_coinOutState.Timer == 0)
                {
                    _totalPauseCount++;
                    if (_totalPauseCount > HopperConsts.HopperMaxTos)
                    {
                        _coinOutState.State = HopperTaskState.WaitingForReset;
                        _totalPauseCount = 0;
                        StopHopperMotor();
                        OnMessageReceived(new HopperFaultStatus { FaultType = HopperFaultTypes.HopperEmpty });
                        WriteConsoleMessage("****************HCD_OUT_OF_COINS_EVENT*****************");

                    }
                    else
                    {
                        _coinOutState.State = HopperTaskState.WaitingForTimeout;
                        _coinOutState.Timer = HopperConsts.HopperPauseTime;
                        MotorOff();
                        WriteConsoleMessage("****************HCD_HOPPER_PAUSE_EVENT*****************");
                    }

                }
            }
        }
        private void ProcessWaitingForTrailingEdge(ChangeRecord record, bool connected)
        {
            if (CheckForDisconnect(connected))
            {
                return;
            }

            if (((int)record.NewValue & (int)HopperMasks.HopperCoinOutMask) == 0)
            {

                _coinOutState.State = HopperTaskState.WaitingForLeadingEdge;
                _totalPauseCount = 0;
                if ((_hopperState.State & HopperMasks.HopperMotorDriveMask) != 0)
                {
                    _coinOutState.Timer = HopperConsts.HopperEmptyTime;
                    OnMessageReceived(new CoinOutStatus { Legal = true });
                    WriteConsoleMessage("****************HCD_COINOUT_EVENT*****************");
                    _currentCoinout++;
                    if (_currentCoinout >= _maxCoinoutAllowed)
                    {
                        StopHopperMotor();
                    }
                }
                else
                {
                    StopHopperMotor();//This extra then linux impl.
                    _currentCoinout++;
                    if (_currentCoinout <= _maxCoinoutAllowed)
                    {
                        OnMessageReceived(new CoinOutStatus { Legal = true });
                    }
                    else
                    {
                        OnMessageReceived(new CoinOutStatus { Legal = false });
                        WriteConsoleMessage("****************HCD_ILLEGAL_COINOUT_EVENT*****************");
                    }
                }
                return;
            }

            if (_coinOutState.Timer == 0)
            {
                _coinOutState.State = HopperTaskState.WaitingForReset;
                _totalPauseCount = 0;
                StopHopperMotor();
                OnMessageReceived(new HopperFaultStatus { FaultType  = HopperFaultTypes.HopperJam });
                WriteConsoleMessage("****************HCD_JAMMED_EVENT*****************");

            }
        }

        private void ProcessWaitingForTimeout(ChangeRecord record, bool connected)
        {
            if (CheckForDisconnect(connected))
            {
                return;
            }

            /* If leading edge */
            if (((int)record.NewValue & (int)HopperMasks.HopperCoinOutMask) != 0)
            {
                _coinOutState.State = HopperTaskState.WaitingForTrailingEdge;
                _coinOutState.Timer = HopperConsts.HopperMaxBlockedTime;
                MotorOn();
                WriteConsoleMessage("****************HCD_LEADING_EDGE_EVENT*****************");
                return;
            }
            if (_coinOutState.Timer == 0)
            {
                _coinOutState.State = HopperTaskState.WaitingForLeadingEdge;
                _coinOutState.Timer = HopperConsts.HopperEmptyTime;
                if ((_hopperState.State & HopperMasks.HopperMotorDriveMask) != 0)
                {
                    MotorOn();
                }
                WriteConsoleMessage("****************HCD_HOPPER_UNPAUSE_EVENT*****************");
            }

        }
        private void ProcessWaitingForReset(ChangeRecord record, bool connected)
        {
            if (_hopperState.IsConnected)
            {
                _hopperState.IsConnected = CheckForDisconnect(connected);
            }

        }

        private bool CheckForDisconnect(bool connected)
        {
            if (!connected)
            {
                _coinOutState.State = HopperTaskState.WaitingForReset;
                _totalPauseCount = 0;
                StopHopperMotor();
                OnMessageReceived(new HopperFaultStatus { FaultType = HopperFaultTypes.HopperDisconnected });
                WriteConsoleMessage("****************HCD_DISCONNECTED_EVENT*****************");
                return true;
            }
            return false;
        }

        private void StartHopperMotor()
        {
            lock (Lock)
            {
                _coinOutState.Timer = HopperConsts.HopperPauseTime;
                _hopperState.State |= HopperMasks.HopperMotorDriveMask;
                MotorOn();
            }
        }

        private void StopHopperMotor()
        {
            lock (Lock)
            {
                _hopperState.State &= (~HopperMasks.HopperMotorDriveMask);
                MotorOff();
            }
        }

        private (bool, bool, ChangeRecord) ReadwithDisconnetTest()
        {
            var connect = true;
            (var ret, var record) = base.Read();
            _testDisconnectTimer.QuadPart += record.ElapsedSinceLastChange.QuadPart;

            if (record.NewValue != record.OldValue)
            {
                _testDisconnectTimer = default;
                _disconnected = false;
                _disconnectCount = 0;
            }
            if (_testDisconnectTimer.QuadPart > HopperConsts.HopperDisconnetTime * 10000)
            {
                var disconnect = SetHopperTestDisconnection();
                if (disconnect)
                {
                    if (!_disconnected && record.NewValue == 0) //If actual disconnect, data from it should be 0
                    {
                        if (_disconnectCount <= HopperConsts.HopperDisconnectThreshold)
                        {
                            _disconnectCount++;
                        }
                        else
                        {
                            _disconnected = disconnect;
                            connect = false;
                            System.Console.WriteLine("Hopper is disconnecetd");
                        }
                    }
                }
                else
                {
                    _disconnectCount = 0;
                    _testDisconnectTimer = default;
                    _disconnected = false;
                }
            }
            return (ret, connect, record);
        }

        private bool SetHopperTestDisconnection()
        {
            var result = 0;
            Ioctl(HopperCommands.HopperTestDisconnection, 0, ref result);
            return (result != 0);
        }

        public bool MotorOn()
        {
            byte value = (byte)HopperMasks.HopperMotorDriveMask;
            return Ioctl(HopperCommands.HopperSetOutputs, value);
        }

        public bool MotorOff()
        {
            byte value = (byte)HopperMasks.HopperMotorDriveMask;
            return Ioctl(HopperCommands.HopperClrOutputs, value);
        }

        public bool Enable()
        {
            return Ioctl(HopperCommands.HopperEnable, 1);
        }

        public bool Disable()
        {
            return Ioctl(HopperCommands.HopperEnable, 0);
        }

        public bool SetType(HopperType type)
        {
            return Ioctl(HopperCommands.HopperSetType, (int)type);
        }

        public bool IsConnected()
        {
            return !_disconnected;
        }

        public byte GetCurrentHopperRegisterValue()
        {
            byte value = 0;
            Ioctl(HopperCommands.HopperGetRegisterValue, (byte)value);
            return value;
        }

        private void WriteConsoleMessage(string msg)
        {
            System.Console.WriteLine(msg);
        }

        /// <summary>Process a GDS message into protocol calls.</summary>
        /// <param name="message">GDS message</param>
        protected void ProcessMessage(GdsSerializableMessage message)
        {
            switch (message)
            {
                case HopperMotorControl control:
                    {
                        if(control.OnOff)
                        {
                            StartHopperMotor();
                        }
                        else
                        {
                            StopHopperMotor();
                        }
                        break;
                    }
                case HopperMaxOutControl control:
                    {
                        SetMaxCoinoutAllowed(control.Count);
                        break;
                    }
                case DeviceReset:
                    {
                        Reset();
                        break;
                    }

                default:
                    if (message.ReportId == GdsConstants.ReportId.DeviceReset)
                    {
                        DeviceInitialize();
                    }
                    break;
            }
        }

        private void DeviceInitialize()
        {
            lock(Lock)
            {
                SetType(Type);
                Disable();
                Enable();
            }
        }

        public bool Reset()
        {
            lock (Lock)
            {
                Disable();
                if (!IsConnected())
                {
                    _coinOutState.State = HopperTaskState.WaitingForReset;
                    OnMessageReceived(new HopperFaultStatus { FaultType = HopperFaultTypes.HopperDisconnected });
                    WriteConsoleMessage("****************HCD_DISCONNECTED_EVENT*****************");
                    _hopperState.IsConnected = false;
                }
                else
                {
                    _coinOutState.State = HopperTaskState.WaitingForLeadingEdge;
                    _hopperState.IsConnected = true;
                    _currentCoinout = 0;
                }
                Enable();
                return true;
            }
        }

        public bool SetMaxCoinoutAllowed(int amount)
        {
            _currentCoinout = _maxCoinoutAllowed = 0;
            lock (Lock)
            {

                _maxCoinoutAllowed = amount;
                return true;
            }
        }

        public byte GetStatusReport()
        {
            lock (Lock)
            {
                return GetCurrentHopperRegisterValue();
            }
        }
    }
}

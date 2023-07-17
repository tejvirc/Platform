namespace Aristocrat.Monaco.Hardware.PWM
{
    using System;
    using System.Reflection;
    using Contracts.PWM;
    using log4net;

    public class CoinAcceptor : PwmDevice<ICoinAcceptor>, ICoinAcceptor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region PwmDevice
        /// <inheritdoc />
        override protected IPwmDevice Device => this;
        #endregion PwmDevice

        #region IPwmDevice
        /// <inheritdoc />
        public virtual PwmDeviceConfig DeviceConfig { get; set; }
        /// <inheritdoc />
        public string Name { get; } = "CC-62";
        public virtual void ConfigureDevice()
        {
            DeviceConfig = new PwmDeviceConfig
            {
                DeviceInterface = new Guid("{e72a476b-664e-4a6b-9439-aa8cfa294ff2}"),
                Mode = CreateFileOption.Overlapped,
                pollingFrequency = 20,//ms
                waitPeriod = 20,//ms
                DeviceType = NativeConstants.CoinAcceptorDeviceType

            };
        }
        /// <inheritdoc />
        public void Initialize()
        {

            ConfigureDevice();
            if (!GetDevice())
            {
                Logger.Error($"{nameof(Device)}: Cannot find device for {DeviceConfig.DeviceInterface}");
                return;
            }

            if (!OpenDevice())
            {
                Logger.Error($"{nameof(Device)}: Cannot create device handle for {DeviceConfig.DeviceInterface}");
                return;
            }
        }
        #endregion IPwmDevice

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable

        #region ICoinAcceptor
        public virtual bool AckRead(uint txnId)
        {
            //Non Non-Volatile Coin acceptor has nothing do with ACK
            return true;
        }

        public bool RejectMechanishOnOff(bool OnOff)
        {
            return Ioctl(OnOff ?
                CoinAcceptorCommands.CoinAcceptorRejectOn
                : CoinAcceptorCommands.CoinAcceptorRejectOff, 0);
        }
        public bool DivertorMechanishOnOff(bool OnOff)
        {
            return Ioctl(OnOff ?
                CoinAcceptorCommands.CoinAcceptorDivertorOn
                : CoinAcceptorCommands.CoinAcceptorDivertorOff, 0);
        }

        public bool StartPolling()
        {
            return Ioctl(CoinAcceptorCommands.CoinAcceptorStartPolling, 0);
        }

        public bool StopPolling()
        {
            return Ioctl(CoinAcceptorCommands.CoinAcceptorStopPolling, 0);
        }

        #endregion ICoinAcceptor
    }
}

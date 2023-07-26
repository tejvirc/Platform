namespace Aristocrat.Monaco.Hardware.Pwm.CoinAcceptor
{
    using System;
    using Contracts.Communicator;
    using Contracts.CoinAcceptor;

    /// <summary>
    ///     Class to manage communication with Volatile coin acceptor device.
    ///     Which us based in pulse width modulated signal <see cref="CoinAcceptorCommunicator".
    ///     Volatile coin acceptor device is based on kernel driver which is saving all the events
    ///     in non persisted memory and in case of power failure will not be recovered.
    ///     Plus read operation is destructive read.
    /// </summary>
    public class CoinAcceptor : CoinAcceptorCommunicator
    {
        private const int PollingFrequency = 20;
        private const int WaitPeriod = 20;
        /// <inheritdoc/>
        public override bool Configure(IComConfiguration comConfiguration)
        {
            DeviceConfig = new PwmDeviceConfig
            {
                DeviceInterface = new Guid("{e72a476b-664e-4a6b-9439-aa8cfa294ff2}"),
                Mode = CreateFileOption.Overlapped,
                PollingFrequency = PollingFrequency,//ms
                WaitPeriod = WaitPeriod,//ms
                DeviceType = NativeConstants.CoinAcceptorDeviceType
            };
            Model = "CC-62";
            Manufacturer = "CC-62";
            return true;
        }
    }
}

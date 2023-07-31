namespace Aristocrat.Monaco.Hardware.Pwm.Hopper
{
    using Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor;
    using Aristocrat.Monaco.Hardware.Contracts.Communicator;
    using Aristocrat.Monaco.Hardware.Contracts.Gds;
    using Aristocrat.Monaco.Hardware.Contracts.Hopper;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Class to manage communication with Volatile coin acceptor device.
    ///     Which us based in pulse width modulated signal <see cref="CoinAcceptorCommunicator".
    ///     Volatile coin acceptor device is based on kernel driver which is saving all the events
    ///     in non persisted memory and in case of power failure will not be recovered.
    ///     Plus read operation is destructive read.
    /// </summary>
    public class Hopper : HopperCommunicator
    {
        private const int PollingFrequency = 20;
        private const int WaitPeriod = 20;
        /// <inheritdoc/>
        public override bool Configure(IComConfiguration comConfiguration)
        {
            DeviceConfig = new PwmDeviceConfig
            {
                DeviceInterface = new Guid("{cfd4b6da-89df-4be7-9deb-eae7479a02aa}"),
                Mode = CreateFileOption.Overlapped,
                PollingFrequency = PollingFrequency,//ms
                WaitPeriod = WaitPeriod,//ms
                DeviceType = NativeConstants.HopperDeviceType
            };
            Model = "Aristocrat";
            Manufacturer = "Aristocrat";
            return true;
        }

        /// <inheritdoc/>
        public override HopperType Type { get; } = HopperType.Aristocrat;
    }
}

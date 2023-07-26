namespace Aristocrat.Monaco.Hardware.Pwm.CoinAcceptor
{
    using Aristocrat.Monaco.Hardware.Contracts.Communicator;
    using Aristocrat.Monaco.Hardware.Contracts.PWM;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public class CoinAcceptor : CoinAcceptorCommunicator
    {
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

    }
}

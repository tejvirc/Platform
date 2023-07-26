using Aristocrat.Monaco.Hardware.Contracts.Communicator;
using Aristocrat.Monaco.Hardware.Contracts.PWM;
using System;

namespace Aristocrat.Monaco.Hardware.Pwm.CoinAcceptor
{
    public class NvCoinAcceptor : CoinAcceptorCommunicator
    {

        public override bool Configure(IComConfiguration comConfiguration)
        {
            DeviceConfig = new PwmDeviceConfig
            {
                DeviceInterface = new Guid("{fc444f85-8973-4e71-8f96-d1a9614fe9d9}"),
                Mode = CreateFileOption.Overlapped,
                pollingFrequency = 20,//ms
                waitPeriod = 20,//ms
                DeviceType = NativeConstants.NVCoinAcceptorDeviceType
            };

            Model = "CC-62";
            Manufacturer = "CC-62";
            return true;
        }
        protected override (bool, ChangeRecord) ReadSync()
        {
            ChangeRecord output = new ChangeRecord();
            var ret = Ioctl(CoinAcceptorCommands.CoinAcceptorPeek, 0, ref output);
            return (ret, output);
        }

        protected override (bool, ChangeRecord) ReadAsync()
        {

            if (DeviceConfig.Mode != CreateFileOption.Overlapped)
            {
                throw new InvalidOperationException();
            }

            ChangeRecord output = new ChangeRecord();
            var ret = IoctlAsync(CoinAcceptorCommands.CoinAcceptorPeek, 0, ref output);
            return (ret, output);
        }
        protected override bool AckRead(uint txnId)
        {
            return Ioctl(CoinAcceptorCommands.CoinAcceptorAcknowledge, txnId);
        }
    }
}

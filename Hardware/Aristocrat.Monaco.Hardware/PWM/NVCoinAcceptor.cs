//namespace Aristocrat.Monaco.Hardware.PWM
//{
//    using System;
//    using Contracts.PWM;

//    public class NVCoinAcceptor : CoinAcceptor
//    {
//        override protected IPwmDevice Device => this;
//        public override PwmDeviceConfig DeviceConfig { get; set; }
//        public override void ConfigureDevice()
//        {
//            DeviceConfig = new PwmDeviceConfig
//            {
//                DeviceInterface = new Guid("{fc444f85-8973-4e71-8f96-d1a9614fe9d9}"),
//                Mode = CreateFileOption.Overlapped,
//                pollingFrequency = 20,//ms
//                waitPeriod = 20,//ms
//                DeviceType = NativeConstants.NVCoinAcceptorDeviceType
//            };

//        }

//        public override (bool, ChangeRecord) ReadSync()
//        {
//            ChangeRecord output = new ChangeRecord();
//            var ret = Ioctl(CoinAcceptorCommands.CoinAcceptorPeek, 0, ref output);
//            return (ret, output);
//        }

//        public override (bool, ChangeRecord) ReadAsync()
//        {

//            if (DeviceConfig.Mode != CreateFileOption.Overlapped)
//            {
//                throw new InvalidOperationException();
//            }

//            ChangeRecord output = new ChangeRecord();
//            var ret = IoctlAsync(CoinAcceptorCommands.CoinAcceptorPeek, 0, ref output);
//            return (ret, output);
//        }
//        public override bool AckRead(uint txnId)
//        {
//            return Ioctl(CoinAcceptorCommands.CoinAcceptorAcknowledge, txnId);
//        }
//    }
//}

namespace Aristocrat.Monaco.G2S.Tests.Stubs
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class PrinterDeviceStub : ClientDeviceBase<printer>
    {
        public PrinterDeviceStub()
            : base(1, null)
        {
            Guests = new List<int> { 3 };
        }

        public override void Open(IStartupContext context)
        {
        }

        public override void Close()
        {
        }

        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
        }
    }
}
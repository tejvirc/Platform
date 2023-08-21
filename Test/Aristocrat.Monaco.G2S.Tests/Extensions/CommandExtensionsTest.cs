namespace Aristocrat.Monaco.G2S.Tests.Extensions
{
    using System.Reflection;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.OptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CommandExtensionsTest
    {
        [TestMethod]
        public void WhenIncludeAllDevicesG2SAllExpectTrue()
        {
            var command = new getOptionList();
            command.deviceClass = DeviceClass.G2S_all;

            Assert.IsTrue(command.IncludeAllDevices());
        }

        [TestMethod]
        public void WhenIncludeAllDevicesG2SSpecifiedDeviceExpectFalse()
        {
            foreach (var deviceClass in typeof(DeviceClass).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (deviceClass.GetValue(null).ToString() == DeviceClass.G2S_all)
                {
                    continue;
                }

                var command = new getOptionList();

                command.deviceClass = deviceClass.GetValue(null).ToString();

                Assert.IsFalse(command.IncludeAllDevices());
            }
        }
    }
}
namespace Aristocrat.Monaco.G2S.Tests.Options
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig.ChangeOptionConfig;
    using G2S.Options;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using DeviceClass = Aristocrat.G2S.DeviceClass;

    [TestClass]
    public class ApplyOptionConfigToOptionConfigDeviceTest : BaseApplyOptionConfigToDeviceTest
    {
        [TestMethod]
        public void WhenApplyExpectSuccess()
        {
            var device = new OptionConfigDevice(DeviceId, _deviceObserverMock.Object);

            Assert.AreEqual(device.ConfigurationId, 0);
            Assert.AreEqual(device.NoResponseTimer, TimeSpan.FromMilliseconds(DefaultNoResponseTimer));
            Assert.IsTrue(device.ConfigComplete);
            Assert.AreEqual(device.ConfigDateTime, default(DateTime));

            _egmMock.Setup(x => x.GetDevice(DeviceClass.G2S_optionConfig.TrimmedDeviceClass(), DeviceId))
                .Returns(device);

            var service = new ApplyOptionConfigToDevicesService(
                _egmMock.Object,
                _profileServiceMock.Object,
                new List<IDeviceOptions>
                {
                    new OptionConfigDeviceOptions()
                });

            service.UpdateDeviceProfiles(CreateChangeOptionConfigRequest(DeviceClass.G2S_optionConfig));

            Assert.AreEqual(device.ConfigurationId, ConfigurationId);
            Assert.AreEqual(device.NoResponseTimer, TimeSpan.FromMilliseconds(NewNoResponseTimer));

            _profileServiceMock.Verify(
                x => x.Save(It.Is<IDevice>(d => d.Id == DeviceId && d.DeviceClass == typeof(optionConfig).Name)),
                Times.Once);
        }

        protected override OptionCurrentValue CreateOptionCurrentValue()
        {
            return new OptionCurrentValue
            {
                ParameterType = OptionConfigParameterType.Complex,
                ChildValues = new List<OptionCurrentValue>
                {
                    new OptionCurrentValue
                    {
                        ParamId = "G2S_configurationId",
                        Value = ConfigurationId.ToString(),
                        ParameterType = OptionConfigParameterType.Integer
                    },
                    new OptionCurrentValue
                    {
                        ParameterType = OptionConfigParameterType.Complex,
                        ChildValues = new List<OptionCurrentValue>
                        {
                            new OptionCurrentValue
                            {
                                ParamId = "G2S_noResponseTimer",
                                Value = NewNoResponseTimer.ToString(),
                                ParameterType = OptionConfigParameterType.Integer
                            },
                            CreateCommonOptionCurrentValue()
                        }
                    }
                }
            };
        }
    }
}
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
    public class ApplyOptionConfigToCabinetDeviceTest : BaseApplyOptionConfigToDeviceTest
    {
        private const int DefaultConfigDelayPeriod = 240000;
        private const int NewConfigDelayPeriod = 250000;

        [TestMethod]
        public void WhenApplyExpectSuccess()
        {
            var device = new CabinetDevice(_deviceObserverMock.Object, _egmStateObserverMock.Object);

            Assert.AreEqual(device.ConfigurationId, 0);
            Assert.IsTrue(device.RestartStatus);
            Assert.IsFalse(device.UseDefaultConfig);
            Assert.IsTrue(device.RequiredForPlay);
            Assert.IsTrue(device.ConfigComplete);
            Assert.AreEqual(device.ConfigDateTime, default(DateTime));
            Assert.AreEqual(device.ConfigDelayPeriod, DefaultConfigDelayPeriod);

            _egmMock.Setup(x => x.GetDevice(DeviceClass.G2S_cabinet.TrimmedDeviceClass(), DeviceId))
                .Returns(device);

            var service = new ApplyOptionConfigToDevicesService(
                _egmMock.Object,
                _profileServiceMock.Object,
                new List<IDeviceOptions>
                {
                    new CabinetDeviceOptions()
                });

            service.UpdateDeviceProfiles(CreateChangeOptionConfigRequest(DeviceClass.G2S_cabinet));

            Assert.AreEqual(device.ConfigurationId, ConfigurationId);
            Assert.IsFalse(device.RestartStatus);
            Assert.IsTrue(device.UseDefaultConfig);
            Assert.IsFalse(device.RequiredForPlay);
            Assert.AreEqual(device.ConfigDelayPeriod, NewConfigDelayPeriod);

            _profileServiceMock.Verify(
                x => x.Save(It.Is<IDevice>(d => d.Id == DeviceId && d.DeviceClass == typeof(cabinet).Name)),
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
                                ParamId = "G2S_restartStatus",
                                Value = "false",
                                ParameterType = OptionConfigParameterType.Boolean
                            },
                            new OptionCurrentValue
                            {
                                ParamId = "G2S_configDelayPeriod",
                                Value = NewConfigDelayPeriod.ToString()
                            },
                            new OptionCurrentValue
                            {
                                ParameterType = OptionConfigParameterType.Complex,
                                ChildValues = new List<OptionCurrentValue>
                                {
                                    new OptionCurrentValue
                                    {
                                        ParamId = "G2S_useDefaultConfig",
                                        Value = "true",
                                        ParameterType = OptionConfigParameterType.Boolean
                                    },
                                    new OptionCurrentValue
                                    {
                                        ParamId = "G2S_requiredForPlay",
                                        Value = "false",
                                        ParameterType = OptionConfigParameterType.Boolean
                                    },
                                    CreateCommonOptionCurrentValue()
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
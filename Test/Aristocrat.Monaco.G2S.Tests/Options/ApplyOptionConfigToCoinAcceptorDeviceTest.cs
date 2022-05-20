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
    public class ApplyOptionConfigToCoinAcceptorDeviceTest : BaseApplyOptionConfigToDeviceTest
    {
        [TestMethod]
        public void WhenApplyExpectSuccess()
        {
            var device = new CoinAcceptorDevice(_deviceObserverMock.Object);

            Assert.AreEqual(device.ConfigurationId, 0);
            Assert.IsTrue(device.RestartStatus);
            Assert.IsFalse(device.UseDefaultConfig);
            Assert.IsFalse(device.RequiredForPlay);
            Assert.IsTrue(device.ConfigComplete);
            Assert.AreEqual(device.ConfigDateTime, default(DateTime));

            _egmMock.Setup(x => x.GetDevice(DeviceClass.G2S_coinAcceptor.TrimmedDeviceClass(), DeviceId))
                .Returns(device);

            var service = new ApplyOptionConfigToDevicesService(
                _egmMock.Object,
                _profileServiceMock.Object,
                new List<IDeviceOptions>
                {
                    new CoinAcceptorDeviceOptions()
                });

            service.UpdateDeviceProfiles(CreateChangeOptionConfigRequest(DeviceClass.G2S_coinAcceptor));

            Assert.AreEqual(device.ConfigurationId, ConfigurationId);
            Assert.IsFalse(device.RestartStatus);
            Assert.IsTrue(device.UseDefaultConfig);
            Assert.IsTrue(device.RequiredForPlay);

            _profileServiceMock.Verify(
                x => x.Save(It.Is<IDevice>(d => d.Id == DeviceId && d.DeviceClass == typeof(coinAcceptor).Name)),
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
                                ParameterType = OptionConfigParameterType.Integer
                            },
                            new OptionCurrentValue
                            {
                                ParameterType = OptionConfigParameterType.Complex,
                                ChildValues = new List<OptionCurrentValue>
                                {
                                    new OptionCurrentValue
                                    {
                                        ParamId = "G2S_useDefaultConfig",
                                        Value = "true"
                                    },
                                    new OptionCurrentValue
                                    {
                                        ParamId = "G2S_requiredForPlay",
                                        Value = "true"
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
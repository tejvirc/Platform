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
    public class ApplyOptionConfigToGatDeviceTest : BaseApplyOptionConfigToDeviceTest
    {
        private const int IdReaderId = 1;

        private const int DefaultMinQueuedComps = 1;

        private const int NewMinQueuedComps = 10;

        [TestMethod]
        public void WhenApplyExpectSuccess()
        {
            var device = new GatDevice(DeviceId, _deviceObserverMock.Object);

            Assert.AreEqual(device.ConfigurationId, 0);
            Assert.IsFalse(device.UseDefaultConfig);
            Assert.AreEqual(device.TimeToLive, DefaultTimeToLive);
            Assert.AreEqual(device.IdReaderId, 0);
            Assert.AreEqual(device.MinQueuedComps, DefaultMinQueuedComps);
            Assert.AreEqual(device.SpecialFunctions, t_g2sBoolean.G2S_false);
            Assert.IsTrue(device.ConfigComplete);
            Assert.AreEqual(device.ConfigDateTime, default(DateTime));

            _egmMock.Setup(x => x.GetDevice(DeviceClass.G2S_gat.TrimmedDeviceClass(), DeviceId))
                .Returns(device);

            var service = new ApplyOptionConfigToDevicesService(
                _egmMock.Object,
                _profileServiceMock.Object,
                new List<IDeviceOptions>
                {
                    new GatDeviceOptions()
                });

            service.UpdateDeviceProfiles(CreateChangeOptionConfigRequest(DeviceClass.G2S_gat));

            Assert.AreEqual(device.ConfigurationId, ConfigurationId);
            Assert.IsTrue(device.UseDefaultConfig);
            Assert.AreEqual(device.TimeToLive, NewTimeToLive);
            Assert.AreEqual(device.IdReaderId, IdReaderId);
            Assert.AreEqual(device.MinQueuedComps, NewMinQueuedComps);
            Assert.AreEqual(device.SpecialFunctions, t_g2sBoolean.G2S_true);

            _profileServiceMock.Verify(
                x => x.Save(It.Is<IDevice>(d => d.Id == DeviceId && d.DeviceClass == typeof(gat).Name)),
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
                                ParamId = "G2S_useDefaultConfig",
                                Value = "true",
                                ParameterType = OptionConfigParameterType.Boolean
                            },
                            new OptionCurrentValue
                            {
                                ParameterType = OptionConfigParameterType.Complex,
                                ChildValues = new List<OptionCurrentValue>
                                {
                                    new OptionCurrentValue
                                    {
                                        ParamId = "G2S_timeToLive",
                                        Value = NewTimeToLive.ToString(),
                                        ParameterType = OptionConfigParameterType.Integer
                                    },
                                    new OptionCurrentValue
                                    {
                                        ParameterType = OptionConfigParameterType.Complex,
                                        ChildValues = new List<OptionCurrentValue>
                                        {
                                            new OptionCurrentValue
                                            {
                                                ParamId = "G2S_idReaderId",
                                                Value = IdReaderId.ToString(),
                                                ParameterType = OptionConfigParameterType.Integer
                                            },
                                            new OptionCurrentValue
                                            {
                                                ParamId = "G2S_minQueuedComps",
                                                Value = NewMinQueuedComps.ToString(),
                                                ParameterType = OptionConfigParameterType.Integer
                                            },
                                            new OptionCurrentValue
                                            {
                                                ParamId = "G2S_specialFunctions",
                                                Value = t_g2sBoolean.G2S_true.ToString()
                                            },
                                            CreateCommonOptionCurrentValue()
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
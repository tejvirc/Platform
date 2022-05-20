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
    public class ApplyOptionConfigToEventHandlerDeviceTest : BaseApplyOptionConfigToDeviceTest
    {
        // TODO: MinLogEntries is hardcoded to 2500 for now
        private const int DefaultMinLogEntries = 2500;
        private const int NewMinLogEntries = 2500;

        [TestMethod]
        public void WhenApplyExpectSuccess()
        {
            _eventPersistenceManagermMock.Setup(x => x.SupportedEvents).Returns(new List<supportedEvent>());

            var device = new EventHandlerDevice(
                DeviceId,
                _deviceObserverMock.Object,
                _eventPersistenceManagermMock.Object);

            Assert.AreEqual(device.ConfigurationId, 0);
            Assert.IsTrue(device.RestartStatus);
            Assert.IsFalse(device.UseDefaultConfig);
            Assert.IsFalse(device.RequiredForPlay);
            Assert.AreEqual(device.MinLogEntries, DefaultMinLogEntries);
            Assert.AreEqual(device.TimeToLive, DefaultTimeToLive);
            Assert.AreEqual(device.QueueBehavior, t_queueBehaviors.G2S_disable);
            Assert.AreEqual(device.DisableBehavior, t_disableBehaviors.G2S_overwrite);
            Assert.IsTrue(device.ConfigComplete);
            Assert.AreEqual(device.ConfigDateTime, default(DateTime));

            _egmMock.Setup(x => x.GetDevice(DeviceClass.G2S_eventHandler.TrimmedDeviceClass(), DeviceId))
                .Returns(device);

            var service = new ApplyOptionConfigToDevicesService(
                _egmMock.Object,
                _profileServiceMock.Object,
                new List<IDeviceOptions>
                {
                    new EventHandlerDeviceOptions(_eventPersistenceManagermMock.Object)
                });

            service.UpdateDeviceProfiles(CreateChangeOptionConfigRequest(DeviceClass.G2S_eventHandler));

            Assert.AreEqual(device.ConfigurationId, ConfigurationId);
            Assert.IsFalse(device.RestartStatus);
            Assert.IsTrue(device.UseDefaultConfig);
            Assert.IsTrue(device.RequiredForPlay);
            Assert.AreEqual(device.MinLogEntries, NewMinLogEntries);
            Assert.AreEqual(device.TimeToLive, NewTimeToLive);
            Assert.AreEqual(device.QueueBehavior, t_queueBehaviors.G2S_discard);
            Assert.AreEqual(device.DisableBehavior, t_disableBehaviors.G2S_discard);

            _profileServiceMock.Verify(
                x => x.Save(It.Is<IDevice>(d => d.Id == DeviceId && d.DeviceClass == typeof(eventHandler).Name)),
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
                                        ParamId = "G2S_requiredForPlay",
                                        Value = "true"
                                    },
                                    new OptionCurrentValue
                                    {
                                        ParamId = "G2S_minLogEntries",
                                        Value = NewMinLogEntries.ToString()
                                    },
                                    new OptionCurrentValue
                                    {
                                        ParameterType = OptionConfigParameterType.Complex,
                                        ChildValues = new List<OptionCurrentValue>
                                        {
                                            new OptionCurrentValue
                                            {
                                                ParamId = "G2S_timeToLive",
                                                Value = NewTimeToLive.ToString()
                                            },
                                            new OptionCurrentValue
                                            {
                                                ParamId = "G2S_queueBehavior",
                                                Value = t_queueBehaviors.G2S_discard.ToString()
                                            },
                                            new OptionCurrentValue
                                            {
                                                ParamId = "G2S_disableBehavior",
                                                Value = t_disableBehaviors.G2S_discard.ToString()
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
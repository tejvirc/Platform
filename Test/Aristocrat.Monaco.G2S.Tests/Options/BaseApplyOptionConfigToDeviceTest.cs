namespace Aristocrat.Monaco.G2S.Tests.Options
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;
    using Data.OptionConfig.ChangeOptionConfig;
    using Data.Profile;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    public abstract class BaseApplyOptionConfigToDeviceTest
    {
        protected const int DeviceId = 1;

        protected const int ConfigurationId = 123;

        protected const double DefaultNoResponseTimer = 300000;

        protected const double NewNoResponseTimer = 400000;

        protected const int DefaultTimeToLive = 30000;

        protected const int NewTimeToLive = 40000;

        protected readonly Mock<IDeviceObserver> _deviceObserverMock = new Mock<IDeviceObserver>();

        protected readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();

        protected readonly Mock<IEgmStateObserver> _egmStateObserverMock = new Mock<IEgmStateObserver>();

        protected readonly Mock<IEventPersistenceManager> _eventPersistenceManagermMock =
            new Mock<IEventPersistenceManager>();

        protected readonly Mock<IEventLift> _eventLiftMock = new Mock<IEventLift>();

        protected readonly Mock<IHostQueue> _hostQueueMock = new Mock<IHostQueue>();

        protected readonly Mock<IProfileService> _profileServiceMock = new Mock<IProfileService>();

        protected readonly DateTime configDateTime = DateTime.Now;

        protected void AssertDeviceConfigDateTime(IDevice device, DateTime configDateTime)
        {
            Assert.AreEqual(device.ConfigDateTime.Year, configDateTime.Year);
            Assert.AreEqual(device.ConfigDateTime.Month, configDateTime.Month);
            Assert.AreEqual(device.ConfigDateTime.Day, configDateTime.Day);
            Assert.AreEqual(device.ConfigDateTime.Hour, configDateTime.Hour);
            Assert.AreEqual(device.ConfigDateTime.Minute, configDateTime.Minute);
        }

        protected ChangeOptionConfigRequest CreateChangeOptionConfigRequest(string deviceClass)
        {
            return new ChangeOptionConfigRequest
            {
                ConfigurationId = ConfigurationId,
                Options = new List<Option>
                {
                    new Option
                    {
                        DeviceId = DeviceId,
                        OptionId = "OptionId",
                        DeviceClass = deviceClass,
                        OptionValues = new OptionCurrentValue[] { CreateOptionCurrentValue() }
                    }
                }
            };
        }

        protected OptionCurrentValue CreateCommonOptionCurrentValue()
        {
            return new OptionCurrentValue
            {
                ParameterType = OptionConfigParameterType.Complex,
                ChildValues =
                    new List<OptionCurrentValue>
                    {
                        new OptionCurrentValue
                        {
                            ParamId = "G2S_configDateTime",
                            Value = configDateTime.ToString()
                        },
                        new OptionCurrentValue
                        {
                            ParamId = "G2S_configComplete",
                            Value = "false"
                        }
                    }
            };
        }

        protected abstract OptionCurrentValue CreateOptionCurrentValue();
    }
}
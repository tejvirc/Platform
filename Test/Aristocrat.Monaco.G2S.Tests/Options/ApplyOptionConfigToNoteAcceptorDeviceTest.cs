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
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using DeviceClass = Aristocrat.G2S.DeviceClass;

    [TestClass]
    public class ApplyOptionConfigToNoteAcceptorDeviceTest : BaseApplyOptionConfigToDeviceTest
    {
        private Mock<IDeviceRegistryService> _registry;
        private Mock<IPersistentStorageManager> _storage;

        [TestInitialize]
        public void Initialize()
        {
            _registry = new Mock<IDeviceRegistryService>();

            var noteAcceptor = new Mock<INoteAcceptor>();
            noteAcceptor.SetupGet(m => m.Denominations).Returns(new List<int>());
            _registry.Setup(m => m.GetDevice<INoteAcceptor>()).Returns(noteAcceptor.Object);

            _storage = new Mock<IPersistentStorageManager>();
        }

        [TestMethod]
        public void WhenApplyExpectSuccess()
        {
            var device = new NoteAcceptorDevice(DeviceId, _deviceObserverMock.Object);

            Assert.AreEqual(device.ConfigurationId, 0);
            Assert.IsTrue(device.RestartStatus);
            Assert.IsFalse(device.UseDefaultConfig);
            Assert.IsTrue(device.RequiredForPlay);
            Assert.IsTrue(device.ConfigComplete);
            Assert.AreEqual(device.ConfigDateTime, default(DateTime));

            _egmMock.Setup(x => x.GetDevice(DeviceClass.G2S_noteAcceptor.TrimmedDeviceClass(), DeviceId))
                .Returns(device);
            var persistence = new Mock<IPersistenceProvider>();
            persistence.Setup(a => a.GetOrCreateBlock(It.IsAny<string>(), It.IsAny<PersistenceLevel>())).Returns(new Mock<IPersistentBlock>().Object);


            var service = new ApplyOptionConfigToDevicesService(
                _egmMock.Object,
                _profileServiceMock.Object,
                new List<IDeviceOptions>
                {
                    new NoteAcceptorDeviceOptions(_registry.Object, persistence.Object, new Mock<IPropertiesManager>().Object)
                });

            service.UpdateDeviceProfiles(
                CreateChangeOptionConfigRequest(DeviceClass.G2S_noteAcceptor));

            Assert.AreEqual(device.ConfigurationId, ConfigurationId);
            Assert.IsFalse(device.RestartStatus);
            Assert.IsTrue(device.UseDefaultConfig);
            Assert.IsTrue(device.RequiredForPlay);

            _profileServiceMock.Verify(
                x => x.Save(It.Is<IDevice>(d => (d.Id == DeviceId) && (d.DeviceClass == typeof(noteAcceptor).Name))),
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
                                        Value = "true",
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

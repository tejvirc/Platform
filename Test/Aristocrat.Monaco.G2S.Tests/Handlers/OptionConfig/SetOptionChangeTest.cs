namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Data.OptionConfig.ChangeOptionConfig;
    using ExpressMapper;
    using G2S.Handlers;
    using G2S.Handlers.OptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Newtonsoft.Json;
    using Test.Common;

    [TestClass]
    public class SetOptionChangeTest : BaseOptionConfigHandlerTest
    {
        private readonly Mock<IMonacoContextFactory> _contextFactoryMock = new Mock<IMonacoContextFactory>();

        private readonly Mock<ISetOptionChangeValidateService> _validationServiceMock =
            new Mock<ISetOptionChangeValidateService>();

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<SetOptionChange>();
        }

        [TestMethod]
        public async Task CheckActiveDevicesTest()
        {
            const string deviceClass = "G2S_optionConfig";

            var firstOptionConfigDevice = new Mock<IOptionConfigDevice>();
            firstOptionConfigDevice.SetupGet(x => x.Id).Returns(1);
            firstOptionConfigDevice.SetupGet(x => x.DeviceClass).Returns(deviceClass);
            firstOptionConfigDevice.SetupGet(x => x.Active).Returns(true);

            var secondOptionConfigDevice = new Mock<IOptionConfigDevice>();
            secondOptionConfigDevice.SetupGet(x => x.Id).Returns(1);
            secondOptionConfigDevice.SetupGet(x => x.Active).Returns(false);

            _egmMock.Setup(x => x.GetDevice<IOptionConfigDevice>(1)).Returns(firstOptionConfigDevice.Object);

            _egmMock.Setup(x => x.GetDevice<IOptionConfigDevice>(2)).Returns(secondOptionConfigDevice.Object);

            var handler = CreateHandler();

            var command = CreateCommand();

            command.Command.option = new[]
            {
                new option { deviceId = 1, deviceClass = deviceClass },
                new option { deviceId = 2, deviceClass = deviceClass }
            };

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, ErrorCode.G2S_OCX017);
        }

        [TestMethod]
        public void MapSimpleSetOptionChangeTest()
        {
            var command = CreateCommand();

            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow.AddDays(1);

            command.Command.option = (option[])CreateOptionsWithSimpleTypes();
            command.Command.configurationId = 33;
            command.Command.startDateTime = startDate;
            command.Command.endDateTime = endDate;
            command.Command.restartAfter = true;

            var incomingRequest = Mapper.Map<setOptionChange, ChangeOptionConfigRequest>(command.Command);

            Assert.AreEqual(incomingRequest.Options.Count(), 4);
            Assert.AreEqual(incomingRequest.ConfigurationId, 33);
            Assert.AreEqual(incomingRequest.StartDateTime, startDate);
            Assert.AreEqual(incomingRequest.EndDateTime, endDate);
            Assert.IsTrue(incomingRequest.RestartAfter);

            var index = 1;
            foreach (var option in incomingRequest.Options)
            {
                Assert.IsNull(option.OptionValues.First().ChildValues);
                Assert.AreEqual(option.DeviceId, index);
                Assert.AreEqual(option.DeviceClass, $"G2S_deviceClass{index}");
                Assert.AreEqual(option.OptionGroupId, $"G2S_optionGroupId{index}");
                Assert.AreEqual(option.OptionId, $"G2S_optionId{index}");

                Assert.AreEqual(option.OptionValues.First().ParamId, $"A{index}B_{index}");

                switch (index)
                {
                    case 1:
                    {
                        Assert.AreEqual(option.OptionValues.First().ParameterType, OptionConfigParameterType.Integer);
                        Assert.AreEqual(int.Parse(option.OptionValues.First().Value), 11);
                        break;
                    }

                    case 2:
                    {
                        Assert.AreEqual(option.OptionValues.First().ParameterType, OptionConfigParameterType.Decimal);
                        Assert.AreEqual(decimal.Parse(option.OptionValues.First().Value), 22);
                        break;
                    }

                    case 3:
                    {
                        Assert.AreEqual(option.OptionValues.First().ParameterType, OptionConfigParameterType.String);
                        Assert.AreEqual(option.OptionValues.First().Value, "33_str");
                        break;
                    }

                    case 4:
                    {
                        Assert.AreEqual(option.OptionValues.First().ParameterType, OptionConfigParameterType.Boolean);
                        Assert.IsTrue(bool.Parse(option.OptionValues.First().Value));
                        break;
                    }
                }

                index++;
            }
        }

        [TestMethod]
        public void MapComplexSetOptionChangeTest()
        {
            var command = CreateCommand();

            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow.AddDays(1);

            command.Command.option = (option[])CreateOptionsWithComplexTypes();
            command.Command.configurationId = 33;
            command.Command.startDateTime = startDate;
            command.Command.endDateTime = endDate;
            command.Command.restartAfter = true;

            var incomingRequest = Mapper.Map<setOptionChange, ChangeOptionConfigRequest>(command.Command);

            Assert.AreEqual(incomingRequest.Options.Count(), 1);
            Assert.AreEqual(
                incomingRequest.Options.ToArray()[0].OptionValues.First().ParameterType,
                OptionConfigParameterType.Complex);

            var rootOptionValue = incomingRequest.Options.ToArray()[0].OptionValues.First();
            Assert.AreEqual(rootOptionValue.ChildValues.Count(), 2);
            Assert.IsNull(rootOptionValue.Value);
            Assert.AreEqual(rootOptionValue.ChildValues.ToArray()[0].ParameterType, OptionConfigParameterType.Integer);

            var firstLevelOptionValue = rootOptionValue.ChildValues.ToArray()[1];
            Assert.AreEqual(firstLevelOptionValue.ChildValues.Count(), 2);
            Assert.IsNull(firstLevelOptionValue.Value);
            Assert.AreEqual(
                firstLevelOptionValue.ChildValues.ToArray()[0].ParameterType,
                OptionConfigParameterType.Decimal);

            var secondLevelOptionValue = firstLevelOptionValue.ChildValues.ToArray()[1];
            Assert.AreEqual(secondLevelOptionValue.ChildValues.Count(), 2);
            Assert.IsNull(secondLevelOptionValue.Value);
            Assert.AreEqual(
                secondLevelOptionValue.ChildValues.ToArray()[0].ParameterType,
                OptionConfigParameterType.String);

            var thirdLevelOptionValue = secondLevelOptionValue.ChildValues.ToArray()[1];
            Assert.AreEqual(thirdLevelOptionValue.ChildValues.Count(), 2);
            Assert.IsNull(thirdLevelOptionValue.Value);

            Assert.AreEqual(
                thirdLevelOptionValue.ChildValues.ToArray()[0].ParameterType,
                OptionConfigParameterType.Boolean);
            Assert.AreEqual(
                thirdLevelOptionValue.ChildValues.ToArray()[1].ParameterType,
                OptionConfigParameterType.Boolean);
        }

        [TestMethod]
        public async Task CheckIfConfiguratorOfDevicesTest()
        {
            ConfigureOptionConfigDevice();

            var changeLog = ConfigureOptionChangeLog();
            _changeLogRepositoryMock.Setup(x => x.GetByTransactionId(It.IsAny<DbContext>(), 1))
                .Returns(changeLog);

            var handler = CreateHandler();

            var command = CreateCommand();
            command.Command.option = (option[])CreateOptionsWithComplexTypes(lastItemValue: false);

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, ErrorCode.G2S_OCX018);
        }

        [TestMethod]
        public async Task HandleCommandNormalFlowTest()
        {
            ConfigureOptionConfigDevice();

            _idProvider.Setup(m => m.GetNextTransactionId()).Returns(1);

            _changeLogRepositoryMock.Setup(x => x.GetPendingChangeLogs(It.IsAny<DbContext>()))
                .Returns(new List<OptionChangeLog> { new OptionChangeLog { TransactionId = 1 } }.AsQueryable());

            var handler = CreateHandlerWithRealBuilder();
            var command = CreateCommand();
            command.Command.option = (option[])CreateOptionsWithComplexTypes();
            command.Command.authorizeList = CreateAuthorizeList();

            var changeLog = CreateOptionChangeLog();
            changeLog.AuthorizeItems = new List<ConfigChangeAuthorizeItem>
            {
                new ConfigChangeAuthorizeItem
                {
                    HostId = 2,
                    TimeoutAction = TimeoutActionType.Abort
                }
            };

            _changeLogRepositoryMock.Setup(x => x.GetByTransactionId(It.IsAny<DbContext>(), It.IsAny<long>()))
                .Returns(changeLog);

            await handler.Handle(command);

            _changeLogRepositoryMock.Verify(
                x =>
                    x.Add(
                        It.IsAny<DbContext>(),
                        It.Is<OptionChangeLog>(log => log.ChangeStatus == ChangeStatus.Pending)),
                Times.Exactly(1));

            var response = command.Responses.FirstOrDefault() as ClassCommand<optionConfig, optionChangeStatus>;

            Assert.IsNotNull(response);

            Assert.AreEqual(response.Command.changeStatus, t_changeStatus.G2S_pending);
            Assert.IsNotNull(response.Command.authorizeStatusList);
            Assert.AreEqual(response.Command.authorizeStatusList.authorizeStatus.Length, 1);
        }

        private authorizeList CreateAuthorizeList(DateTime? timeoutDate = null)
        {
            var authorizeList = new authorizeList
            {
                authorizeItem =
                    new[]
                    {
                        new authorizeItem
                        {
                            hostId = 1,
                            timeoutDate = timeoutDate ?? DateTime.MinValue,
                            timeoutAction =
                                t_timeoutActionTypes.G2S_abort
                        }
                    }
            };

            return authorizeList;
        }

        private void ConfigureOptionConfigDevice()
        {
            var optionConfigDevice = new Mock<IOptionConfigDevice>();
            optionConfigDevice.SetupGet(x => x.Id).Returns(1);
            optionConfigDevice.SetupGet(x => x.Active).Returns(true);
            optionConfigDevice.SetupGet(x => x.DeviceClass).Returns("deviceClass1");
            _egmMock.Setup(x => x.GetDevice<IOptionConfigDevice>(1)).Returns(optionConfigDevice.Object);
            _egmMock.Setup(x => x.GetDevice("deviceClass1", 1)).Returns(optionConfigDevice.Object);
        }

        private OptionChangeLog ConfigureOptionChangeLog()
        {
            var optionChangeLog = CreateOptionChangeLog();

            var command = CreateCommand();

            command.Command.option = (option[])CreateOptionsWithComplexTypes();
            command.Command.configurationId = 1;
            command.Command.startDateTime = startDateTime;
            command.Command.endDateTime = endDateTime;
            command.Command.restartAfter = true;
            command.Command.applyCondition = ApplyCondition.Disable.ToG2SString();
            command.Command.disableCondition = DisableCondition.Immediate.ToG2SString();

            var incomingRequest = Mapper.Map<setOptionChange, ChangeOptionConfigRequest>(command.Command);

            optionChangeLog.ChangeData = JsonConvert.SerializeObject(incomingRequest);

            return optionChangeLog;
        }

        private SetOptionChange CreateHandler(IG2SEgm egm = null)
        {
            var handler = new SetOptionChange(
                egm ?? _egmMock.Object,
                _commandBuilderMock.Object,
                _changeLogRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _eventLiftMock.Object,
                _contextFactoryMock.Object,
                _validationServiceMock.Object,
                _idProvider.Object);

            return handler;
        }

        private SetOptionChange CreateHandlerWithRealBuilder()
        {
            var builder = new OptionChangeStatusCommandBuilder(
                _changeLogRepositoryMock.Object,
                _contextFactoryMock.Object);

            var handler = new SetOptionChange(
                _egmMock.Object,
                builder,
                _changeLogRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _eventLiftMock.Object,
                _contextFactoryMock.Object,
                _validationServiceMock.Object,
                _idProvider.Object);

            return handler;
        }

        private ClassCommand<optionConfig, setOptionChange> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, setOptionChange>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.configurationId = 1;
            command.Command.startDateTime = startDateTime;
            command.Command.endDateTime = endDateTime;
            command.Command.restartAfter = true;
            command.Command.applyCondition = ApplyCondition.Disable.ToG2SString();
            command.Command.disableCondition = DisableCondition.Immediate.ToG2SString();

            return command;
        }

        private IEnumerable<option> CreateOptionsWithComplexTypes(
            string lastItemParameterName = "G2S_booleanValue2",
            bool lastItemValue = true)
        {
            return new[]
            {
                new option
                {
                    deviceId = 1,
                    deviceClass = "G2S_deviceClass1",
                    optionGroupId = "G2S_optionGroupId1",
                    optionId = "G2S_optionId1",
                    optionCurrentValues =
                        new optionCurrentValues
                        {
                            Items = new object[]
                            {
                                new complexValue
                                {
                                    paramId = "G2S_complex1",
                                    Items =
                                        new object[]
                                        {
                                            new integerValue1
                                            {
                                                Value = 11,
                                                paramId = "G2S_integerValue1"
                                            },
                                            new complexValue
                                            {
                                                paramId = "G2S_complex2",
                                                Items = new object[]
                                                {
                                                    new decimalValue1
                                                    {
                                                        paramId = "G2S_decimalValue1",
                                                        Value = 22
                                                    },
                                                    new complexValue
                                                    {
                                                        paramId = "G2S_complex3",
                                                        Items = new object[]
                                                        {
                                                            new stringValue1
                                                            {
                                                                paramId = "G2S_stringValue1",
                                                                Value = "str"
                                                            },
                                                            new complexValue
                                                            {
                                                                paramId = "G2S_complex4",
                                                                Items = new object[]
                                                                {
                                                                    new booleanValue1
                                                                    {
                                                                        paramId = "G2S_booleanValue1",
                                                                        Value = true
                                                                    },
                                                                    new booleanValue1
                                                                    {
                                                                        paramId = lastItemParameterName,
                                                                        Value = lastItemValue
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                }
                            }
                        }
                }
            };
        }

        private IEnumerable<option> CreateOptionsWithSimpleTypes()
        {
            return new[]
            {
                new option
                {
                    deviceId = 1,
                    deviceClass = "G2S_deviceClass1",
                    optionGroupId = "G2S_optionGroupId1",
                    optionId = "G2S_optionId1",
                    optionCurrentValues =
                        new optionCurrentValues
                        {
                            Items = new object[]
                            {
                                new integerValue1
                                {
                                    Value = 11,
                                    paramId = "A1B_1"
                                }
                            }
                        }
                },
                new option
                {
                    deviceId = 2,
                    deviceClass = "G2S_deviceClass2",
                    optionGroupId = "G2S_optionGroupId2",
                    optionId = "G2S_optionId2",
                    optionCurrentValues =
                        new optionCurrentValues
                        {
                            Items = new object[]
                            {
                                new decimalValue1
                                {
                                    Value = 22,
                                    paramId = "A2B_2"
                                }
                            }
                        }
                },
                new option
                {
                    deviceId = 3,
                    deviceClass = "G2S_deviceClass3",
                    optionGroupId = "G2S_optionGroupId3",
                    optionId = "G2S_optionId3",
                    optionCurrentValues =
                        new optionCurrentValues
                        {
                            Items = new object[]
                            {
                                new stringValue1
                                {
                                    Value = "33_str",
                                    paramId = "A3B_3"
                                }
                            }
                        }
                },
                new option
                {
                    deviceId = 4,
                    deviceClass = "G2S_deviceClass4",
                    optionGroupId = "G2S_optionGroupId4",
                    optionId = "G2S_optionId4",
                    optionCurrentValues =
                        new optionCurrentValues
                        {
                            Items = new object[]
                            {
                                new booleanValue1
                                {
                                    Value = true,
                                    paramId = "A4B_4"
                                }
                            }
                        }
                }
            };
        }
    }
}
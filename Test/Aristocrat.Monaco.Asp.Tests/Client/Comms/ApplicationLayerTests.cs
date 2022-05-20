namespace Aristocrat.Monaco.Asp.Tests.Client.Comms
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Aristocrat.Monaco.Asp.Client.Devices;
    using Aristocrat.Monaco.Asp.Client.Utilities;
    using Aristocrat.Monaco.Asp.Events;
    using Asp.Client.Comms;
    using Asp.Client.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    public class AppLayerTestDataContext : TestDataContext
    {
        public CommPortMock PortMock { get; } = new CommPortMock();
        public ApplicationLayer ApplicationLayer { get; set; }
        public Mock<IParameterProcessor> ProcessorMock { get; set; }
        public Dictionary<string, object> TestState { get; } = new Dictionary<string, object>();
        public Mock<ISystemDisableManager> SystemDisableManagerMock { get; set; }

        public Mock<IReportableEventsManager> ReportableEventsManagerMock { get; set; } = new Mock<IReportableEventsManager>();

        public Mock<IEventBus> EventBusMock { get; set; }
    }

    [TestClass]
    public class ApplicationLayerTests : AspUnitTestBase<AppLayerTestDataContext>
    {
        private static readonly Guid ProgressiveLinkDownGuid = new Guid("{A9ED9439-946E-4FCC-97D0-121EC418A771}");
        private string DefaultClassTypeParam => $"{DefaultDeviceClass:X2}-{DefaultDeviceType:X2}-{DefaultParameter:X2}";
        protected const int DefaultDeviceMulticastClass = 3;
        private string DefaultMulticastClassTypeParam => $"{DefaultDeviceMulticastClass:X2}-{DefaultDeviceType:X2}-{DefaultParameter:X2}";
        private CommPortMock PortMock => TestData.PortMock;
        private ApplicationLayer ApplicationLayer => TestData.ApplicationLayer;
        private Mock<IParameterProcessor> ProcessorMock => TestData.ProcessorMock;
        private Mock<IParameterFactory> ParameterFactoryMock { get; set; }

        private static List<(byte @class, byte type, byte parameter)> _activeReportableEvents = new List<(byte @class, byte type, byte parameter)>
        {
            (1, 2, 3),
            (4, 5, 6),
            (7, 8, 9),
            (255, 255, 255),
        };

        private void SetupApplicationLayer(bool mockFactory = false, List<(byte @class, byte type, byte parameter)> activeReportableEvents = null)
        {
            SetupDataSource();
            SetupParameterFactory();
            ParameterFactoryMock = MockRepository.Create<IParameterFactory>(MockBehavior.Loose);
            TestData.ProcessorMock = MockRepository.Create<IParameterProcessor>();
            TestData.SystemDisableManagerMock = MockRepository.Create<ISystemDisableManager>();
            TestData.SystemDisableManagerMock.Setup(x => x.Enable(ProgressiveLinkDownGuid));
            TestData.EventBusMock = new Mock<IEventBus>(MockBehavior.Strict);
            TestData.EventBusMock.Setup(x => x.Publish(It.IsAny<LinkStatusChangedEvent>()));

            if (activeReportableEvents != null)
            {
                ParameterFactoryMock.Setup(s => s.Create(activeReportableEvents[0].@class, activeReportableEvents[0].type, activeReportableEvents[0].parameter))
                    .Returns(new Parameter(CreatePrototype(activeReportableEvents[0].@class, activeReportableEvents[0].type, activeReportableEvents[0].parameter).Object))
                    .Verifiable();

                ParameterFactoryMock.Setup(s => s.Create(activeReportableEvents[1].@class, activeReportableEvents[1].type, activeReportableEvents[1].parameter))
                    .Returns(new Parameter(CreatePrototype(activeReportableEvents[1].@class, activeReportableEvents[1].type, activeReportableEvents[1].parameter).Object))
                    .Verifiable();

                ParameterFactoryMock.Setup(s => s.Create(activeReportableEvents[2].@class, activeReportableEvents[2].type, activeReportableEvents[2].parameter))
                    .Returns(new Parameter(CreatePrototype(activeReportableEvents[2].@class, activeReportableEvents[2].type, activeReportableEvents[2].parameter).Object))
                    .Verifiable();

                ParameterFactoryMock.Setup(s => s.Create(activeReportableEvents[3].@class, activeReportableEvents[3].type, activeReportableEvents[3].parameter))
                    .Returns(new Parameter(CreatePrototype(activeReportableEvents[3].@class, activeReportableEvents[3].type, activeReportableEvents[3].parameter).Object))
                    .Verifiable();

                TestData.ProcessorMock.Setup(s => s.SetEvent(It.IsAny<IParameter>())).Returns(() => true).Verifiable();
            }

            TestData.ReportableEventsManagerMock.Setup(s => s.GetAll()).Returns(() => (activeReportableEvents != null && activeReportableEvents.Any() ? activeReportableEvents : new List<(byte @class, byte type, byte parameter)>()).ToImmutableList());

            TestData.ApplicationLayer = new ApplicationLayer(
                PortMock,
                ProcessorMock.Object,
                mockFactory ? ParameterFactoryMock.Object : ParameterFactory,
                TestData.SystemDisableManagerMock.Object,
                TestData.EventBusMock.Object,
                TestData.ReportableEventsManagerMock.Object);
            Assert.IsTrue(ApplicationLayer.Start("TEST"));
            Assert.IsTrue(WaitFor(200, () => ApplicationLayer.IsLinkUp));
        }

        private Mock<IParameterPrototype> CreatePrototype(byte @class, byte type, byte parameter)
        {
            var mockPrototype = new Mock<IParameterPrototype>();

            var mockClassId = new Mock<INamedId>();
            mockClassId.SetupGet(s => s.Id).Returns(() => @class);

            var mockTypeId = new Mock<INamedId>();
            mockTypeId.SetupGet(s => s.Id).Returns(() => type);

            mockPrototype.SetupGet(s => s.ClassId).Returns(() => mockClassId.Object);
            mockPrototype.SetupGet(s => s.TypeId).Returns(() => mockTypeId.Object);
            mockPrototype.SetupGet(s => s.Id).Returns(() => parameter);

            mockPrototype.SetupGet(s => s.FieldsPrototype).Returns(() => new List<IFieldPrototype>());

            return mockPrototype;
        }

        private static bool ValidateParam(IParameterPrototype p1, IParameterPrototype p2)
        {
            Assert.AreEqual(p1.ClassId.Id, p2.ClassId.Id);
            Assert.AreEqual(p1.TypeId.Id, p2.TypeId.Id);
            Assert.AreEqual(p1.Id, p2.Id);
            return true;
        }

        private void SetupExpectedReceivedData(IList<string> expectedData)
        {
            PortMock.DataReceivedAction = data =>
            {
                if (data.Count > 4)
                {
                    if (expectedData.Count > 0)
                    {
                        var range = data.GetRange(2, data.Count - 4);
                        var matchCount = Regex.Matches(BitConverter.ToString(range.ToArray()), expectedData.First())
                            .Count;
                        Assert.IsTrue(matchCount > 0);
                        expectedData.RemoveAt(0);

                        var count = 0;
                        if (TestData.TestState.TryGetValue("DataReceivedCount", out var value))
                        {
                            count = (int)value;
                        }

                        TestData.TestState["DataReceivedCount"] = count + matchCount;
                    }
                }

                Debug.WriteLine(BitConverter.ToString(data.ToArray()));
                return data.Count;
            };
        }

        private void SetupProcessorMockFor(IParameterPrototype param, AppCommandTypes type)
        {
            switch (type)
            {
                case AppCommandTypes.SetParameter:
                    ProcessorMock.Setup(x => x.SetParameter(It.IsAny<IParameter>())).Returns<IParameter>(
                        x =>
                        {
                            if (!TestData.TestState.TryGetValue("SetParameterFirst", out _))
                            {
                                TestData.TestState.Add("SetParameterFirst", true);
                                return false;
                            }

                            return ValidateParam(param, x);
                        });
                    break;
                case AppCommandTypes.GetParameter:

                    ProcessorMock.Setup(x => x.GetParameter(It.IsAny<IParameter>())).Returns<IParameter>(
                        x =>
                        {
                            ValidateParam(param, x);
                            x.Fields[0].Value = 3;
                            return x;
                        });
                    break;
                case AppCommandTypes.SetEvent:
                    ProcessorMock.Setup(x => x.SetEvent(It.IsAny<IParameter>())).Returns<IParameter>(
                        x =>
                        {
                            if (!TestData.TestState.TryGetValue("SetEventFirst", out _))
                            {
                                TestData.TestState.Add("SetEventFirst", true);
                                return false;
                            }

                            return ValidateParam(param, x);
                        });
                    break;
                case AppCommandTypes.ClearEvent:
                    ProcessorMock.Setup(x => x.ClearEvent(It.IsAny<IParameter>())).Returns<IParameter>(
                        x =>
                        {
                            if (!TestData.TestState.TryGetValue("ClearEventFirst", out _))
                            {
                                TestData.TestState.Add("ClearEventFirst", true);
                                return false;
                            }

                            return ValidateParam(param, x);
                        });
                    break;
            }
        }

        private void VerifyProcessorMockFor(AppCommandTypes type, Times times)
        {
            switch (type)
            {
                case AppCommandTypes.SetParameter:
                    ProcessorMock.Verify(x => x.SetParameter(It.IsAny<IParameter>()), times);
                    break;
                case AppCommandTypes.GetParameter:
                    ProcessorMock.Verify(x => x.GetParameter(It.IsAny<IParameter>()), times);
                    break;
                case AppCommandTypes.SetEvent:
                    ProcessorMock.Verify(x => x.SetEvent(It.IsAny<IParameter>()), times);
                    break;
                case AppCommandTypes.ClearEvent:
                    ProcessorMock.Verify(x => x.ClearEvent(It.IsAny<IParameter>()), times);
                    break;
            }
        }

        private void TestCommand(AppCommandTypes type)
        {
            var expectedData = new Dictionary<AppCommandTypes, List<string>>
            {
                {
                    AppCommandTypes.SetParameter,
                    new List<string>
                    {
                        $"06-31-81-{DefaultClassTypeParam}-05-00", $"06-31-81-{DefaultClassTypeParam}-00-00"
                    }
                },
                {
                    AppCommandTypes.GetParameter,
                    new List<string>
                    {
                        $"07-31-82-{DefaultClassTypeParam}-00-03", $"07-31-82-{DefaultClassTypeParam}-00-03"
                    }
                },
                {
                    AppCommandTypes.SetEvent,
                    new List<string>
                    {
                        $"06-31-84-{DefaultClassTypeParam}-04-00", $"06-31-84-{DefaultClassTypeParam}-00-00"
                    }
                },
                {
                    AppCommandTypes.ClearEvent,
                    new List<string>
                    {
                        $"06-31-85-{DefaultClassTypeParam}-04-00", $"06-31-85-{DefaultClassTypeParam}-00-00"
                    }
                },
                {
                    (AppCommandTypes)8,
                    new List<string>
                    {
                        $"06-31-08-{DefaultClassTypeParam}-01-00", $"06-31-08-{DefaultClassTypeParam}-01-00"
                    }
                }
            };
            SetupApplicationLayer();
            var testParam = ParameterFactory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter);
            SetupProcessorMockFor(testParam, type);
            SetupExpectedReceivedData(expectedData[type]);

            using (ApplicationLayer)
            {
                SetupAppGetDataFunc(type, testParam, 2);

                Assert.AreEqual(2, WaitForReceivedCount(2));
            }

            VerifyProcessorMockFor(type, Times.Exactly(2));
        }

        private static bool ValidateSharedDeviceTypeEventReportParam(IParameterPrototype p1, IParameterPrototype p2)
        {
            Assert.AreEqual(p1.ClassId.Id, p2.ClassId.Id);
            Assert.IsTrue(new int[] { 1, 2, 3 }.Contains(p2.TypeId.Id));
            Assert.AreEqual(p1.Id, p2.Id);
            return true;
        }

        private void SetupSharedDeviceTypeEventReportProcessorMockFor(IParameterPrototype param, AppCommandTypes type)
        {
            switch (type)
            {
                case AppCommandTypes.SetEvent:
                    ProcessorMock.Setup(x => x.SetEvent(It.IsAny<IParameter>())).Returns<IParameter>(
                        x =>
                        {
                            if (!TestData.TestState.TryGetValue("SetEventFirst", out _))
                            {
                                TestData.TestState.Add("SetEventFirst", true);
                                return false;
                            }

                            return ValidateSharedDeviceTypeEventReportParam(param, x);
                        });
                    break;
                case AppCommandTypes.ClearEvent:
                    ProcessorMock.Setup(x => x.ClearEvent(It.IsAny<IParameter>())).Returns<IParameter>(
                        x =>
                        {
                            if (!TestData.TestState.TryGetValue("ClearEventFirst", out _))
                            {
                                TestData.TestState.Add("ClearEventFirst", true);
                                return false;
                            }

                            return ValidateSharedDeviceTypeEventReportParam(param, x);
                        });
                    break;
            }
        }

        private void TestSharedDeviceTypeEventReportCommand(AppCommandTypes type)
        {
            var expectedData = new Dictionary<AppCommandTypes, List<string>>
            {
                {
                    AppCommandTypes.SetEvent,
                    new List<string>
                    {
                        $"06-31-84-{DefaultMulticastClassTypeParam}-04-00", $"06-31-84-{DefaultMulticastClassTypeParam}-00-00"
                    }
                },
                {
                    AppCommandTypes.ClearEvent,
                    new List<string>
                    {
                        $"06-31-85-{DefaultMulticastClassTypeParam}-04-00", $"06-31-85-{DefaultMulticastClassTypeParam}-00-00"
                    }
                }
            };
            SetupApplicationLayer();
            var testParam = ParameterFactory.Create(DefaultDeviceMulticastClass, DefaultDeviceType, DefaultParameter);
            SetupSharedDeviceTypeEventReportProcessorMockFor(testParam, type);
            SetupExpectedReceivedData(expectedData[type]);

            using (ApplicationLayer)
            {
                SetupAppGetDataFunc(type, testParam, 2);

                Assert.AreEqual(2, WaitForReceivedCount(2));
            }
            // expect 6 calls for 3 types under the Class 3 with same parameter, see TestDevices.xml
            VerifyProcessorMockFor(type, Times.Exactly(6));
        }

        private void SetupAppGetDataFunc(AppCommandTypes type, IParameterPrototype testParam, int dataCount)
        {
            var sentCount = 0;
            PortMock.GetAppDataFunc = () =>
            {
                if (sentCount >= dataCount)
                {
                    return new List<byte>();
                }

                var data = new List<byte>
                {
                    (byte)type, // Get parameter
                    (byte)testParam.ClassId.Id, // Class
                    (byte)testParam.TypeId.Id, // Type
                    (byte)testParam.Id // Parameter
                };
                sentCount++;
                return data;
            };
            Assert.IsTrue(WaitFor(500, () => sentCount >= dataCount));
        }

        private int WaitForReceivedCount(int minCount, int milliseconds = 500)
        {
            WaitFor(
                milliseconds,
                () => TestData.TestState.TryGetValue("DataReceivedCount", out var count) && (int)count > minCount);
            TestData.TestState.TryGetValue("DataReceivedCount", out var receivedCount);
            return Convert.ToInt32(receivedCount);
        }

        [TestMethod]
        public void ApplicationLayerInitialisesReportableEvents()
        {
            SetupApplicationLayer(true, _activeReportableEvents);

            TestData.ReportableEventsManagerMock.Verify(v => v.GetAll(), Times.Once);

            ParameterFactoryMock.Verify(s => s.Create(_activeReportableEvents[0].@class, _activeReportableEvents[0].type, _activeReportableEvents[0].parameter), Times.Once);
            ParameterFactoryMock.Verify(s => s.Create(_activeReportableEvents[1].@class, _activeReportableEvents[1].type, _activeReportableEvents[1].parameter), Times.Once);
            ParameterFactoryMock.Verify(s => s.Create(_activeReportableEvents[2].@class, _activeReportableEvents[2].type, _activeReportableEvents[2].parameter), Times.Once);
            ParameterFactoryMock.Verify(s => s.Create(_activeReportableEvents[3].@class, _activeReportableEvents[3].type, _activeReportableEvents[3].parameter), Times.Once);

            TestData.ProcessorMock.Verify(v => v.SetEvent(It.IsAny<IParameter>()), Times.Exactly(_activeReportableEvents.Count));
            TestData.ReportableEventsManagerMock.Verify(v => v.SetBatch(_activeReportableEvents), Times.Once);
        }

        [TestMethod]
        public void ApplicationMessageTest()
        {
            var transportMessage = new TransportMessage(new ByteBuffer(new byte[254]));
            var applicationMessage = new ApplicationMessage();
            applicationMessage.Reset(transportMessage);

            applicationMessage.Command = AppCommandTypes.ClearEvent;
            applicationMessage.Class = 2;
            applicationMessage.Type = 3;
            applicationMessage.Param = 4;

            Assert.AreEqual(AppCommandTypes.ClearEvent, applicationMessage.Command);
            Assert.AreEqual((byte)2, applicationMessage.Class);
            Assert.AreEqual((byte)3, applicationMessage.Type);
            Assert.AreEqual((byte)4, applicationMessage.Param);
        }

        [TestMethod]
        public void ApplicationLayerTestCommands()
        {
            var types = Enum.GetValues(typeof(AppCommandTypes));
            foreach (var item in types)
            {
                TestData.TestState.Clear();
                TestCommand((AppCommandTypes)item);
            }
        }

        [TestMethod]
        public void ApplicationLayerGetParameterTest()
        {
            TestCommand(AppCommandTypes.GetParameter);
        }

        [TestMethod]
        public void ApplicationLayerSetParameterTest()
        {
            TestCommand(AppCommandTypes.SetParameter);
        }

        [TestMethod]
        public void ApplicationLayerSetEventCommandTest()
        {
            TestCommand(AppCommandTypes.SetEvent);
            TestData.ReportableEventsManagerMock.Verify(v => v.Set(2, 1, 1), Times.Once);
        }

        [TestMethod]
        public void ApplicationLayerSetSharedDeviceTypeEventReportCommandTest()
        {
            TestSharedDeviceTypeEventReportCommand(AppCommandTypes.SetEvent);
        }

        [TestMethod]
        public void ApplicationLayerClearEventCommandTest()
        {
            TestCommand(AppCommandTypes.ClearEvent);
            TestData.ReportableEventsManagerMock.Verify(v => v.Clear(2, 1, 1), Times.Once);
        }

        [TestMethod]
        public void ApplicationLayerClearSharedDeviceTypeEventReportCommandTest()
        {
            TestSharedDeviceTypeEventReportCommand(AppCommandTypes.ClearEvent);
        }

        [TestMethod]
        public void ApplicationLayerUnsupportedCommandTest()
        {
            TestCommand((AppCommandTypes)8);
        }

        [TestMethod]
        public void ApplicationLayerUnsupportedTest()
        {
            SetupApplicationLayer(true);
            ParameterFactoryMock.SetupSequence(x => x.Exists(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((false, false, false))
                .Returns((true, false, false))
                .Returns((true, true, false));
            var testParam = ParameterFactory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter);

            SetupExpectedReceivedData(
                new List<string>
                {
                    $"06-31-82-{DefaultClassTypeParam}-02-00",
                    $"06-31-82-{DefaultClassTypeParam}-03-00",
                    $"06-31-82-{DefaultClassTypeParam}-04-00"
                });

            using (ApplicationLayer)
            {
                SetupAppGetDataFunc(AppCommandTypes.GetParameter, testParam, 3);
                Assert.AreEqual(3, WaitForReceivedCount(3));
            }
        }

        private void SetupControlDataFunc(ControlData controlData)
        {
            var oldFn = PortMock.GetTransportDataFunc;
            var dataSent = false;
            PortMock.GetTransportDataFunc = () =>
            {
                PortMock.GetTransportDataFunc = oldFn;
                dataSent = true;
                return new List<byte> { 0x02, 0x33, (byte)controlData, 0 };
            };

            Assert.IsTrue(WaitFor(500, () => dataSent));
        }

        [TestMethod]
        public void ApplicationEventResponseTest()
        {
            var appEventReponse = new AppEventResponse
            {
                Class = 1,
                Type = 2,
                Param = 3,
                Command = AppResponseTypes.ClearEventAck,
                TimeStamp = 1234,
                DataSize = 5,
                ResponseStatus = AppResponseStatus.ValidResponse
            };

            Assert.AreEqual((byte)1, appEventReponse.Class);
            Assert.AreEqual((byte)2, appEventReponse.Type);
            Assert.AreEqual((byte)3, appEventReponse.Param);
            Assert.AreEqual(AppResponseTypes.ClearEventAck, appEventReponse.Command);
            Assert.AreEqual((uint)1234, appEventReponse.TimeStamp);
            Assert.AreEqual(5, appEventReponse.DataSize);
            Assert.AreEqual(AppResponseStatus.ValidResponse, appEventReponse.ResponseStatus);
        }

        [TestMethod]
        public void ApplicationLayerEventTest()
        {
            SetupApplicationLayer();
            var testParam = ParameterFactory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter);
            SetupProcessorMockFor(testParam, AppCommandTypes.SetEvent);
            var expectedReceivedData = new List<string>
            {
                $"06-31-84-{DefaultClassTypeParam}-04-00",
                $"06-31-84-{DefaultClassTypeParam}-00-00",
                "02-34-01-00",
                $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)"
            };
            SetupExpectedReceivedData(expectedReceivedData);

            using (ApplicationLayer)
            {
                SetupAppGetDataFunc(AppCommandTypes.SetEvent, testParam, 2);
                for (var i = 0; i < 200; i++)
                {
                    ProcessorMock.Raise(
                        p => p.ParameterEvent += null,
                        ProcessorMock.Object,
                        ParameterFactory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter));
                }

                Assert.AreEqual(2, WaitForReceivedCount(2));

                SetupControlDataFunc(ControlData.ResumeSendingEvents);
                Assert.AreEqual(103, WaitForReceivedCount(103));

                expectedReceivedData.Clear();
                expectedReceivedData.Add("02-34-02-00");
                SetupControlDataFunc(ControlData.StopSendingEvents);
                Assert.AreEqual(104, WaitForReceivedCount(104));

                TestData.TestState["DataReceivedCount"] = 0;
                ProcessorMock.Raise(
                    p => p.ParameterEvent += null,
                    ProcessorMock.Object,
                    ParameterFactory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter));
                Assert.IsTrue(
                    WaitForReceivedCount(1, 50) == 0);
            }

            VerifyProcessorMockFor(AppCommandTypes.SetEvent, Times.Exactly(2));
        }

        [TestMethod]
        public void TransportLayerLinkDownEventsTest()
        {
            SetupServiceManager();
            SetupApplicationLayer();
            SetupExpectedReceivedData(
                new List<string>
                {
                    $"06-31-84-{DefaultClassTypeParam}-04-00",
                    $"06-31-84-{DefaultClassTypeParam}-00-00",
                    "02-34-01-00",
                    $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                    $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                    $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                    $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                    $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                    $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                    $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                    $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)",
                    $"(0A-32-80-{DefaultClassTypeParam}-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-00)"
                });
            var testParam = ParameterFactory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter);
            SetupProcessorMockFor(testParam, AppCommandTypes.SetEvent);
            TestData.SystemDisableManagerMock.Setup(
                x => x.Disable(
                    ProgressiveLinkDownGuid,
                    SystemDisablePriority.Normal,
                    It.IsAny<Func<string>>(),
                    null));
            using (ApplicationLayer)
            {
                SetupAppGetDataFunc(AppCommandTypes.SetEvent, testParam, 2);
                Assert.AreEqual(2, WaitForReceivedCount(2));
                SetupControlDataFunc(ControlData.ResumeSendingEvents);
                Assert.AreEqual(3, WaitForReceivedCount(3));
                var oldFn = TestData.PortMock.GetDataLinkDataFunc;
                TestData.PortMock.GetDataLinkDataFunc = null;
                Assert.IsTrue(WaitFor(1000, () => !ApplicationLayer.IsLinkUp));

                for (var i = 0; i < 200; i++)
                {
                    ProcessorMock.Raise(
                        p => p.ParameterEvent += null,
                        ProcessorMock.Object,
                        ParameterFactory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter));
                }

                TestData.PortMock.GetDataLinkDataFunc = oldFn;
                Assert.IsTrue(WaitFor(2000, () => ApplicationLayer.IsLinkUp));
                Assert.AreEqual(103, WaitForReceivedCount(103));
            }
        }

        [TestMethod]
        public void TransportLayerInvalidCommandAckTest()
        {
            SetupApplicationLayer();
            SetupExpectedReceivedData(
                new List<string> { "02-34-00-00" });
            using (ApplicationLayer)
            {
                SetupControlDataFunc(ControlData.ReceivedInvalidCommandAckData);

                Assert.AreEqual(1, WaitForReceivedCount(1));
            }
        }

        [TestMethod]
        public void TransportLayerInvalidControlTest()
        {
            SetupApplicationLayer();
            SetupExpectedReceivedData(
                new List<string> { "02-34-00-00" });
            using (ApplicationLayer)
            {
                SetupControlDataFunc((ControlData)8);

                Assert.AreEqual(1, WaitForReceivedCount(1));
            }
        }

        private static void SetupServiceManager()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);
        }
    }
}
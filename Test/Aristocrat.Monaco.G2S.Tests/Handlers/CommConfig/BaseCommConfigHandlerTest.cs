namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.CommConfig;
    using Data.Model;
    using G2S.Handlers;
    using G2S.Services;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using Moq;
    using Newtonsoft.Json;

    public abstract class BaseCommConfigHandlerTest
    {
        protected readonly Mock<ICommChangeLogRepository> ChangeLogRepositoryMock =
            new Mock<ICommChangeLogRepository>();

        protected readonly Mock<ICommandBuilder<ICommConfigDevice, commChangeStatus>> CommandBuilderMock =
            new Mock<ICommandBuilder<ICommConfigDevice, commChangeStatus>>();

        protected readonly Mock<ICommChangeLogValidationService> CommChangeLogValidationServiceMock =
            new Mock<ICommChangeLogValidationService>();

        protected readonly Mock<ICommConfigDevice> ConfigDeviceMock = new Mock<ICommConfigDevice>();

        protected readonly Mock<IMonacoContextFactory> ContextFactoryMock = new Mock<IMonacoContextFactory>();

        protected readonly Mock<IG2SEgm> EgmMock = new Mock<IG2SEgm>();

        protected readonly DateTime EndDateTime = DateTime.UtcNow.AddHours(2);

        protected readonly Mock<IEventLift> EventLiftMock = new Mock<IEventLift>();

        protected readonly Mock<ICommHostConfigItemRepository> HostConfigItemRepositoryMock =
            new Mock<ICommHostConfigItemRepository>();

        protected readonly Mock<IIdProvider> IdProvider = new Mock<IIdProvider>();

        protected readonly DateTime StartDateTime = DateTime.UtcNow.AddHours(-2);

        protected readonly Mock<ITaskScheduler> TaskSchedulerMock = new Mock<ITaskScheduler>();

        protected void ConfigureConfigDeviceMock()
        {
            ConfigDeviceMock.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            ConfigDeviceMock.SetupGet(d => d.Id).Returns(TestConstants.HostId);
        }

        protected IEnumerable<ConfigChangeAuthorizeItem> CreateAuthorizeItems()
        {
            var authorizeItems = new List<ConfigChangeAuthorizeItem>
            {
                new ConfigChangeAuthorizeItem
                {
                    HostId = 1,
                    AuthorizeStatus
                        =
                        AuthorizationState
                            .Pending
                },
                new ConfigChangeAuthorizeItem
                {
                    HostId = 2,
                    AuthorizeStatus
                        =
                        AuthorizationState
                            .Pending
                }
            };

            return authorizeItems;
        }

        private ChangeCommConfigRequest CreateChangeCommConfigRequest(DateTime startDateTime, DateTime endDateTime)
        {
            var changeCommConfigRequest = new ChangeCommConfigRequest
            {
                ConfigurationId = 1,
                ApplyCondition = "G2S_disable",
                DisableCondition = "G2S_immediate",
                RestartAfter = true,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                SetHostItems = new List<SetHostItem>
                {
                    new SetHostItem
                    {
                        HostId = 1,
                        HostIndex = 1,
                        AllowMulticast = true,
                        HostRegistered = false,
                        HostLocation = "location_1",
                        DisplayCommFault = false,
                        UseDefaultConfig = true,
                        NoResponseTimer = 2,
                        RequiredForPlay = true,
                        TimeToLive = 1,
                        ConfigDevices = new List<DeviceSelect>
                        {
                            new DeviceSelect
                            {
                                DeviceId = 1,
                                DeviceActive = true,
                                DeviceClass = "G2S_communications"
                            }
                        },
                        OwnedDevices = new List<DeviceSelect>
                        {
                            new DeviceSelect
                            {
                                DeviceId = 1,
                                DeviceActive = true,
                                DeviceClass = "G2S_eventHandler"
                            }
                        },
                        GuestDevices = new List<DeviceSelect>
                        {
                            new DeviceSelect
                            {
                                DeviceId = 1,
                                DeviceActive = true,
                                DeviceClass = "G2S_meters"
                            }
                        }
                    }
                }
            };

            return changeCommConfigRequest;
        }

        protected CommChangeLog CreateCommChangeLog()
        {
            var commChangeLog = new CommChangeLog(1)
            {
                ConfigurationId = 1,
                TransactionId = 1,
                DeviceId = 1,
                ApplyCondition = ApplyCondition.Disable,
                DisableCondition = DisableCondition.Immediate,
                StartDateTime = StartDateTime,
                EndDateTime = EndDateTime,
                RestartAfter = true,
                ChangeStatus = ChangeStatus.Pending,
                ChangeDateTime = DateTime.UtcNow,
                EgmActionConfirmed = false,
                ChangeException = ChangeExceptionErrorCode.Successful,
                ChangeData = string.Empty
            };

            commChangeLog.ChangeData =
                JsonConvert.SerializeObject(CreateChangeCommConfigRequest(StartDateTime, EndDateTime));

            return commChangeLog;
        }
    }
}
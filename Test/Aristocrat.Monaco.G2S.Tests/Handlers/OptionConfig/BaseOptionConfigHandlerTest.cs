namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using G2S.Handlers;
    using G2S.Services;
    using Monaco.Common.Scheduler;
    using Moq;

    public abstract class BaseOptionConfigHandlerTest
    {
        protected readonly Mock<IOptionChangeLogRepository> _changeLogRepositoryMock =
            new Mock<IOptionChangeLogRepository>();

        protected readonly Mock<ICommandBuilder<IOptionConfigDevice, optionChangeStatus>> _commandBuilderMock =
            new Mock<ICommandBuilder<IOptionConfigDevice, optionChangeStatus>>();

        protected readonly Mock<IOptionConfigDevice> _configDeviceMock = new Mock<IOptionConfigDevice>();

        protected readonly Mock<IConfigurationService> _configServiceMock =
            new Mock<IConfigurationService>();

        protected readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();

        protected readonly Mock<IEventLift> _eventLiftMock = new Mock<IEventLift>();

        protected readonly Mock<IIdProvider> _idProvider = new Mock<IIdProvider>();

        protected readonly Mock<ITaskScheduler> _taskSchedulerMock = new Mock<ITaskScheduler>();

        protected readonly DateTime endDateTime = DateTime.UtcNow.AddHours(2);

        protected readonly DateTime startDateTime = DateTime.UtcNow.AddHours(-2);

        protected OptionChangeLog CreateOptionChangeLog()
        {
            var optionChangeLog = new OptionChangeLog(1)
            {
                TransactionId = 1,
                ConfigurationId = 1,
                DeviceId = 1,
                ApplyCondition = ApplyCondition.Disable,
                DisableCondition = DisableCondition.Immediate,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                RestartAfter = true,
                ChangeStatus = ChangeStatus.Pending,
                ChangeDateTime = DateTime.UtcNow,
                EgmActionConfirmed = false,
                ChangeException = ChangeExceptionErrorCode.Successful,
                ChangeData = string.Empty
            };

            return optionChangeLog;
        }

        protected IEnumerable<ConfigChangeAuthorizeItem> CreateAuthorizeItems()
        {
            var authorizeItems = new List<ConfigChangeAuthorizeItem>
            {
                new ConfigChangeAuthorizeItem
                {
                    HostId = 1,
                    AuthorizeStatus = AuthorizationState.Pending
                },
                new ConfigChangeAuthorizeItem
                {
                    HostId = 2,
                    AuthorizeStatus = AuthorizationState.Pending
                }
            };

            return authorizeItems;
        }
    }
}
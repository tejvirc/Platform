namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Bingo.Client;
    using Aristocrat.Bingo.Client.Configuration;
    using Aristocrat.Bingo.Client.Messages;
    using Commands;
    using Common;
    using Common.Events;
    using Common.Exceptions;
    using Kernel;
    using Localization.Properties;
    using Monaco.Common;
    using Protocol.Common.Storage.Entity;
    using Stateless;

    public sealed class ProgressiveClientConnectionState : BaseClientConnectionState<ProgressiveClient>, IProgressiveClientConnectionState
    {
        private readonly IProgressiveCommandHandlerFactory _progressiveCommandFactory;
        private readonly IProgressiveCommandService _progressiveCommandService;

        public ProgressiveClientConnectionState(
            IEventBus eventBus,
            IEnumerable<IClient> clients,
            IClientConfigurationProvider configurationProvider,
            IPropertiesManager propertiesManager,
            ISystemDisableManager systemDisable,
            IUnitOfWorkFactory unitOfWorkFactory,
            IProgressiveCommandHandlerFactory progressiveCommandFactory,
            IProgressiveCommandService progressiveCommandService) :
            base(eventBus, clients, configurationProvider, propertiesManager, systemDisable, unitOfWorkFactory)
        {
            _progressiveCommandFactory = progressiveCommandFactory ?? throw new ArgumentNullException(nameof(progressiveCommandFactory));
            _progressiveCommandService = progressiveCommandService ?? throw new ArgumentNullException(nameof(progressiveCommandService));

            Initialize();
        }

        private bool IsCrossGameProgressiveEnabled => UnitOfWorkFactory.IsCrossGameProgressiveEnabledForMainGame(PropertiesManager);

        protected override async Task OnRegistering()
        {
            try
            {
                if (IsCrossGameProgressiveEnabled)
                {
                    await _progressiveCommandFactory.Execute(new ProgressiveRegistrationCommand(), TokenSource.Token).ConfigureAwait(false);
                }
            }
            catch (RegistrationException exception)
            {
                Logger.Error("Progressive client registration failed", exception);
                await RegistrationState.FireAsync(FailedRegistrationTrigger, exception.Reason).ConfigureAwait(false);
            }

            await base.OnRegistering();
        }

        protected override void OnRegisteringExit(StateMachine<State, Trigger>.Transition t)
        {
            if (t.Trigger != Trigger.Registered)
            {
                return;
            }

            SystemDisable.Enable(BingoConstants.ProgressiveHostRegistrationFailedKey);
        }

        protected override void OnRegisteringFailed()
        {
            EventBus.Publish(new RegistrationFailedEvent());
            SystemDisable.Disable(
                BingoConstants.ProgressiveHostRegistrationFailedKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveHostRegistrationFailed),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveHostRegistrationFailedHelp));
        }

        protected override  void OnConnected()
        {
            EventBus.Publish(new ProgressiveHostOnlineEvent());
            SystemDisable.Enable(BingoConstants.ProgressiveHostOfflineKey);

            base.OnConnected();

            if (IsCrossGameProgressiveEnabled)
            {
                var gameConfiguration = UnitOfWorkFactory.GetSelectedGameConfiguration(PropertiesManager);
                var gameTitleId = (int)(gameConfiguration?.GameTitleId ?? 0);
                _progressiveCommandService.HandleCommands(
                    PropertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty),
                    gameTitleId,
                    TokenSource.Token).FireAndForget();
            }
        }

        protected override async Task OnDisconnected()
        {
            EventBus.Publish(new ProgressiveHostOfflineEvent());
            SystemDisable.Disable(
                BingoConstants.ProgressiveHostOfflineKey,
                SystemDisablePriority.Normal,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveHostDisconnected),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveHostDisconnectedHelp));

            await base.OnDisconnected();
        }
    }
}
namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Aristocrat.Bingo.Client;
    using Aristocrat.Bingo.Client.Configuration;
    using Commands;
    using Common;
    using Common.Exceptions;
    using Common.Events;
    using Kernel;
    using Localization.Properties;
    using Monaco.Common;
    using Protocol.Common.Storage.Entity;
    using Stateless;

    public class BingoClientConnectionState : BaseClientConnectionState<BingoClient>, IBingoClientConnectionState
    {
        private readonly ICommandHandlerFactory _commandFactory;

        public BingoClientConnectionState(
            IEventBus eventBus,
            IEnumerable<IClient> clients,
            IClientConfigurationProvider configurationProvider,
            IPropertiesManager propertiesManager,
            ISystemDisableManager systemDisable,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommandHandlerFactory commandFactory) :
            base(eventBus, clients, configurationProvider, propertiesManager, systemDisable, unitOfWorkFactory)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));

            Initialize();
        }

        protected override async Task OnRegistering()
        {
            try
            {
                await _commandFactory.Execute(new RegistrationCommand(), TokenSource.Token).ConfigureAwait(false);
            }
            catch (RegistrationException exception)
            {
                Logger.Error("Bingo client registration failed", exception);
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

            SystemDisable.Enable(BingoConstants.BingoHostRegistrationFailedKey);
        }

        protected override void OnRegisteringFailed()
        {
            EventBus.Publish(new RegistrationFailedEvent());
            SystemDisable.Disable(
                BingoConstants.BingoHostRegistrationFailedKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoHostRegistrationFailed),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoHostRegistrationFailedHelp));
        }

        protected override async Task OnConfiguring()
        {
            try
            {
                await _commandFactory.Execute(new ConfigureCommand(), TokenSource.Token).ConfigureAwait(false);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error("Bingo client configuration failed", exception);
                await RegistrationState.FireAsync(FailedConfigurationTrigger, exception.Reason).ConfigureAwait(false);
            }

            await base.OnConfiguring();
        }

        protected override void OnConfiguringExit(StateMachine<State, Trigger>.Transition t)
        {
            if (t.Trigger != Trigger.Configured)
            {
                return;
            }

            SystemDisable.Enable(BingoConstants.BingoHostConfigurationInvalidKey);
            SystemDisable.Enable(BingoConstants.BingoHostConfigurationMismatchKey);
        }

        protected override void OnConnected()
        {
            EventBus.Publish(new HostConnectedEvent());
            SystemDisable.Enable(BingoConstants.BingoHostDisconnectedKey);

            base.OnConnected();

            _commandFactory.Execute(new ClientConnectedCommand(), TokenSource.Token).FireAndForget();
        }

        protected override async Task OnDisconnected()
        {
            EventBus.Publish(new HostDisconnectedEvent());
            SystemDisable.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoHostDisconnected),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoHostDisconnectedHelp));

            await base.OnDisconnected();
        }
    }
}
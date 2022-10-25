namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.Profile;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using Services;

    /// <summary>
    ///     Handler for the <see cref="resetProcessor" /> command
    /// </summary>
    public class ResetProcessor : ICommandHandler<cabinet, resetProcessor>
    {
        private readonly IEventBus _bus;
        private readonly IDisableConditionSaga _disableSaga;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IProfileService _profiles;

        private readonly TimeSpan _resetDelay = TimeSpan.FromSeconds(1);

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResetProcessor" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="disableSaga">An <see cref="IDisableConditionSaga" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        /// <param name="bus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="profiles">An <see cref="IProfileService" /> instance.</param>
        public ResetProcessor(
            IG2SEgm egm,
            IDisableConditionSaga disableSaga,
            IEventLift eventLift,
            IEventBus bus,
            IProfileService profiles)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _disableSaga = disableSaga ?? throw new ArgumentNullException(nameof(disableSaga));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<cabinet, resetProcessor> command)
        {
            return await Sanction.OnlyOwner<ICabinetDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, resetProcessor> command)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();

            if (cabinet.Device == cabinet)
            {
                command.GenerateResponse<resetStarted>();

                Reset();
            }
            else
            {
                _disableSaga.Enter(
                    cabinet,
                    DisableCondition.Immediate,
                    TimeSpan.FromMilliseconds(command.IClass.timeToLive),
                    () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ResetProcessor),
                    success =>
                    {
                        if (success)
                        {
                            command.GenerateResponse<resetStarted>();
                            cabinet.Queue.SendResponse(command);

                            Reset();
                        }
                        else
                        {
                            command.Error.SetErrorCode(ErrorCode.G2S_CBX002);
                            cabinet.Queue.SendResponse(command);
                        }
                    },
                    EgmState.EgmLocked);
            }

            await Task.CompletedTask;
        }

        private void Reset()
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();

            // We need to delay this slightly to ensure event delivery after the response
            Task.Delay(_resetDelay).ContinueWith(
                _ =>
                {
                    cabinet.ProcessorReset = true;
                    _profiles.Save(cabinet);

                    _eventLift.Report(cabinet, EventCode.G2S_CBE401);

                    _bus.Publish(new ExitRequestedEvent(ExitAction.RestartPlatform));
                });
        }
    }
}

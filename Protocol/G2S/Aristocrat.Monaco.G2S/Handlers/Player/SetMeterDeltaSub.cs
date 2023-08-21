namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Profile;
    using Gaming.Contracts.Session;
    using Kernel;

    public class SetMeterDeltaSub : ICommandHandler<player, setMeterDeltaSub>, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IG2SEgm _egm;
        private readonly IPlayerService _players;
        private readonly IProfileService _profiles;

        private bool _disposed;

        public SetMeterDeltaSub(IG2SEgm egm, IPlayerService players, IProfileService profiles, IEventBus bus)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task<Error> Verify(ClassCommand<player, setMeterDeltaSub> command)
        {
            return await Sanction.OnlyOwner<IPlayerDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<player, setMeterDeltaSub> command)
        {
            var device = _egm.GetDevice<IPlayerDevice>();

            var subscription = command.Command.meterDeltaHostSubscription;

            if (!_players.HasActiveSession)
            {
                command.GenerateResponse<setMeterDeltaSubAck>();

                SetSubscription(device, subscription);
            }
            else
            {
                // NOTE:  We're not persisting the pending subscription.  Not sure if this is expected or not

                _bus.Unsubscribe<SessionEndedEvent>(this);
                _bus.Subscribe<SessionEndedEvent>(
                    this,
                    _ =>
                    {
                        _bus.Unsubscribe<SessionEndedEvent>(this);

                        SetSubscription(device, subscription);

                        command.GenerateResponse<setMeterDeltaSubAck>();
                        device.Queue.SendResponse(command);
                    });
            }

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void SetSubscription(IPlayerDevice device, IEnumerable<meterDeltaHostSubscription> subscription)
        {
            device.SetMeterSubscription(subscription);

            _profiles.Save(device);
        }
    }
}

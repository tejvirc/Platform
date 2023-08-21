namespace Aristocrat.Monaco.Mgam.Commands
{
    using System;
    using Aristocrat.Mgam.Client.Messaging;
    using Common;
    using Common.Events;
    using Kernel;

    /// <summary>
    ///     CommandHandlerBase
    /// </summary>
    public abstract class CommandHandlerBase
    {
        protected readonly IEventBus Bus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandHandlerBase" /> class.
        /// </summary>
        /// <param name="bus"><see cref="IEventBus" />.</param>
        protected CommandHandlerBase(IEventBus bus)
        {
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <summary>
        ///     Checks for invalid server response codes that require re-connection.
        /// </summary>
        /// <param name="code"><see cref="IServerResponseCode" />.</param>
        protected void ValidateResponseCode(IServerResponseCode code)
        {
            if (code == null)
            {
                Bus.Publish(new ForceDisconnectEvent(DisconnectReason.InvalidServerResponse));
            }

            switch (code?.ResponseCode)
            {
                case ServerResponseCode.DeviceStillRegisteredWithLauncherSvc:
                case ServerResponseCode.InvalidInstanceId:
                case ServerResponseCode.VltServiceNotRegistered:
                case ServerResponseCode.ServerError:
                    Bus.Publish(new ForceDisconnectEvent(DisconnectReason.InvalidServerResponse));
                    break;
            }
        }
    }
}
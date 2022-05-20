namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Messaging;
    using Cabinet.Contracts;
    using Common;
    using Gaming.Contracts.InfoBar;
    using Kernel;

    /// <summary>
    ///     Handles <see cref="Sign"/> message.
    /// </summary>
    public class SignHandler : MessageHandler<Sign>
    {
        public static readonly Guid InfoBarMessageHandle = new Guid("{93705216-7033-4A10-A33E-AEE30CD6D8FE}");
		private static readonly TimeSpan SignMessageTimeout = TimeSpan.FromSeconds(28);
		
        private readonly IEventBus _bus;

        /// <summary>
        ///     Construct a <see cref="SignHandler"/> object.
        /// </summary>
        /// <param name="bus"><see cref="IEventBus"/></param>
        public SignHandler(IEventBus bus)
        {
            _bus = bus;
        }

        /// <inheritdoc />
        public override Task<IResponse> Handle(Sign message)
        {
            // TODO: if there is physical sign, direct the message there.

            if (!string.IsNullOrEmpty(message.Message))
            {
                var infoBarEvent = new InfoBarDisplayTransientMessageEvent(
                    InfoBarMessageHandle,
                    message.Message,
                    SignMessageTimeout,
                    MgamConstants.PlayerMessageDefaultTextColor,
                    MgamConstants.PlayerMessageDefaultBackgroundColor,
                    InfoBarRegion.Center,
                    DisplayRole.VBD);
                _bus.Publish(infoBarEvent);
            }
            else
            {
                var clearInfoBarEvent = new InfoBarClearMessageEvent(
                    InfoBarMessageHandle,
                    DisplayRole.VBD,
                    InfoBarRegion.Center);
                _bus.Publish(clearInfoBarEvent);
            }

            return Task.FromResult(Ok<SignResponse>());
        }
    }
}

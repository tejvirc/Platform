namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Messaging;
    using Localization.Properties;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Handle the <see cref="MalformedMessage"/> message.
    /// </summary>
    public class MalformedMessageHandler : MessageHandler<MalformedMessage>
    {
        private readonly ILockup _lockup;
        private readonly INotificationLift _notification;
        private readonly ILocalization _localization;

        /// <summary>
        ///     Construct <see cref="MalformedMessageHandler"/>.
        /// </summary>
        /// <param name="lockup">Instance of <see cref="ILockup"/>.</param>
        /// <param name="notification">Instance of <see cref="INotificationLift"/>.</param>
        /// <param name="localization"><see cref="ILocalization"/>.</param>
        public MalformedMessageHandler(
            ILockup lockup,
            INotificationLift notification,
            ILocalization localization)
        {
            _lockup = lockup;
            _notification = notification;
            _localization = localization;
        }

        ///<inheritdoc />
        public override async Task<IResponse> Handle(MalformedMessage message)
        {
            _lockup.LockupForEmployeeCard(_localization.For(CultureFor.Player).GetString(ResourceKeys.VLTCommunicationError));

            await _notification.Notify(NotificationCode.LockedMalformedMessage, message.ErrorDescription);

            return await Task.FromResult(Ok<MalformedMessageResponse>());
        }
    }
}

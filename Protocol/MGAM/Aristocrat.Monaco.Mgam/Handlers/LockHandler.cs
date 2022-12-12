namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Messaging;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Handle the Lock message.
    /// </summary>
    public class LockHandler : MessageHandler<Lock>
    {
        private readonly ILockup _lockup;
        private readonly INotificationLift _notification;

        /// <summary>
        ///     Construct <see cref="LockHandler"/>.
        /// </summary>
        /// <param name="lockup">Instance of <see cref="ILockup"/>.</param>
        /// <param name="notification">Instance of <see cref="INotificationLift"/>.</param>
        public LockHandler(
            ILockup lockup,
            INotificationLift notification)
        {
            _lockup = lockup;
            _notification = notification;
        }

        ///<inheritdoc />
        public override async Task<IResponse> Handle(Lock message)
        {
            _lockup.AddHostLock(
                string.IsNullOrEmpty(message.Message)
                    ? Localizer.GetString(ResourceKeys.LockedByHost, CultureProviderType.Player)
                    : message.Message);

            await _notification.Notify(NotificationCode.LockedCommanded);

            return await Task.FromResult(Ok<LockResponse>());
        }
    }
}

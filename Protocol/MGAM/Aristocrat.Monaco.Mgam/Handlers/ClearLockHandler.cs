namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Messaging;
    using Services.Lockup;
    using Services.Notification;

    /// <summary>
    ///     Handle the ClearLock message.
    /// </summary>
    public class ClearLockHandler : MessageHandler<ClearLock>
    {
        private readonly ILockup _lockup;
        private readonly INotificationLift _notification;

        /// <summary>
        ///     Construct <see cref="ClearLockHandler" />.
        /// </summary>
        /// <param name="lockup">Instance of <see cref="ILockup" />.</param>
        /// <param name="notification">Instance of <see cref="INotificationLift" />.</param>
        public ClearLockHandler(
            ILockup lockup,
            INotificationLift notification)
        {
            _lockup = lockup;
            _notification = notification;
        }

        ///<inheritdoc />
        public override async Task<IResponse> Handle(ClearLock message)
        {
            _lockup.ClearHostLock();

            await _notification.Notify(NotificationCode.LockCleared);

            return await Task.FromResult(Ok<ClearLockResponse>());
        }
    }
}
namespace Aristocrat.Monaco.Mgam.Services.Notification
{
    using System;
    using System.Linq;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     
    /// </summary>
    internal sealed class NotificationQueue : INotificationQueue
    {
        private const int MaxNotifications = 20;

        private readonly ILogger _logger;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        private readonly object _queueLock = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotificationQueue"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="unitOfWorkFactory"></param>
        public NotificationQueue(
            ILogger<NotificationQueue> logger,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _logger = logger;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        /// <inheritdoc />
        public void Enqueue(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            _logger.LogDebug($"Queueing notification {notification.NotificationId}");

            lock (_queueLock)
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var repository = unitOfWork.Repository<Common.Data.Models.Notification>();

                    while (repository.Queryable().Count() >= MaxNotifications)
                    {
                        repository.Delete(repository.Queryable().First());
                    }

                    repository.Add(
                        new Common.Data.Models.Notification
                        {
                            NotificationId = notification.NotificationId,
                            Parameter = notification.Parameter
                        });

                    unitOfWork.SaveChanges();
                } 
            }
        }

        /// <inheritdoc />
        public bool TryDequeue(out Notification notification)
        {
            notification = null;

            lock (_queueLock)
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var repository = unitOfWork.Repository<Common.Data.Models.Notification>();

                    if (!repository.Entities.Any())
                    {
                        return false;
                    }

                    var n = repository.Queryable().First();

                    repository.Delete(n);

                    notification = new Notification
                    {
                        NotificationId = n.NotificationId, Parameter = n.Parameter
                    };

                    return true;
                }
            }
        }

        /// <inheritdoc />
        public bool TryPeek(out Notification notification)
        {
            notification = null;

            lock (_queueLock)
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var repository = unitOfWork.Repository<Common.Data.Models.Notification>();

                    if (!repository.Queryable().Any())
                    {
                        return false;
                    }

                    var n = repository.Queryable().First();

                    notification = new Notification { NotificationId = n.NotificationId, Parameter = n.Parameter };

                    return true;
                }
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            lock (_queueLock)
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var repository = unitOfWork.Repository<Common.Data.Models.Notification>();

                    foreach (var notification in repository.Queryable())
                    {
                        repository.Delete(notification);
                    }

                    unitOfWork.SaveChanges();
                }
            }
        }
    }
}

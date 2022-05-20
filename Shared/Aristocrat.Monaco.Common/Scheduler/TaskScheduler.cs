namespace Aristocrat.Monaco.Common.Scheduler
{
    using System;
    using System.Collections.Specialized;
    using Quartz;
    using Quartz.Impl;
    using SimpleInjector;

    /// <summary>
    ///     Default implementation of task scheduler.
    /// </summary>
    public class TaskScheduler : ITaskScheduler, IDisposable
    {
        private bool _disposed;
        private IScheduler _scheduler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskScheduler" /> class.
        /// </summary>
        /// <param name="container">Dependency injection container.</param>
        public TaskScheduler(Container container)
        {
            var properties = new NameValueCollection { { "quartz.threadPool.threadCount", "2" } };

            var factory = new StdSchedulerFactory(properties);

            _scheduler = factory.GetScheduler().Result;
            _scheduler.JobFactory = new SimpleInjectorJobFactory(container);
            _scheduler.Start();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void ScheduleTask(ITaskSchedulerJob job, DateTime startTime)
        {
            var jobDetails = JobAdapter.ConvertJob(job);
            var trigger = TriggerFactory.GetStartTrigger(startTime);
            ScheduleTask(jobDetails, trigger);
        }

        /// <inheritdoc />
        public void ScheduleTask(ITaskSchedulerJob job, string identity, DateTime startTime)
        {
            var jobDetails = JobAdapter.ConvertJob(job);
            var trigger = TriggerFactory.GetTrigger(identity, startTime);
            ScheduleTask(jobDetails, trigger);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _scheduler?.Shutdown();
            }

            _scheduler = null;

            _disposed = true;
        }

        private void ScheduleTask(IJobDetail jobDetails, ITrigger trigger)
        {
            if (_scheduler.GetTrigger(trigger.Key).Result != null)
            {
                _scheduler.RescheduleJob(trigger.Key, trigger);
            }
            else
            {
                _scheduler.ScheduleJob(jobDetails, trigger);
            }
        }
    }
}
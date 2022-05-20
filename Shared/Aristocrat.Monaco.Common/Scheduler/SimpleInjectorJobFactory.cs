namespace Aristocrat.Monaco.Common.Scheduler
{
    using Quartz;
    using Quartz.Spi;
    using SimpleInjector;

    /// <summary>
    ///     SimpleInjector and Quartz integration.
    /// </summary>
    public class SimpleInjectorJobFactory : IJobFactory
    {
        private readonly Container _container;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SimpleInjectorJobFactory" /> class.
        /// </summary>
        /// <param name="container">Dependency injection container.</param>
        public SimpleInjectorJobFactory(Container container)
        {
            _container = container;
        }

        /// <summary>
        ///     Instantiate job instance.
        /// </summary>
        /// <param name="bundle">Trigger instance.</param>
        /// <param name="scheduler">Scheduler instance.</param>
        /// <returns>Returns instance of job.</returns>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return new JobAdapter(_container);
        }

        /// <summary>
        ///     Allows the job factory to destroy/cleanup the job if needed.
        /// </summary>
        /// <param name="job">Job instance.</param>
        public void ReturnJob(IJob job)
        {
        }
    }
}
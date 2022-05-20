namespace Aristocrat.Monaco.Common.Scheduler
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using log4net;
    using Quartz;
    using SimpleInjector;

    /// <summary>
    ///     Base implementation of task scheduler job adapter.
    /// </summary>
    public class JobAdapter : IJob
    {
        private const string JobFullTypeNameKey = "JobFullTypeNameKey";

        private const string JobSerializedDataKey = "JobSerializedDataKey";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Container _container;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JobAdapter" /> class.
        /// </summary>
        /// <param name="container">Dependency injection container.</param>
        public JobAdapter(Container container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public Task Execute(IJobExecutionContext context)
        {
            ITaskSchedulerJob instance = null;

            var dataMap = context.JobDetail.JobDataMap;

            var data = dataMap.GetString(JobSerializedDataKey);
            var fullTypeName = dataMap.GetString(JobFullTypeNameKey);
            if (string.IsNullOrEmpty(fullTypeName) == false)
            {
                var type = Type.GetType(fullTypeName);
                if (type != null)
                {
                    try
                    {
                        instance = (ITaskSchedulerJob)_container.GetInstance(type);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error("[TaskScheduler] Unable to instantiate scheduler task.", exc);
                        throw;
                    }
                }
            }

            if (instance != null)
            {
                DeserializeJobData(instance, data);
                instance.Execute(GetContext());
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Converts task scheduler job into Quartz job details to be used in Quartz.
        /// </summary>
        /// <param name="job">Current task scheduler job to convert.</param>
        /// <returns>The <see cref="IJobDetail" /></returns>
        public static IJobDetail ConvertJob(ITaskSchedulerJob job)
        {
            return JobBuilder.Create(typeof(JobAdapter))
                .UsingJobData(JobFullTypeNameKey, job.GetType().AssemblyQualifiedName)
                .UsingJobData(JobSerializedDataKey, SerializeJobData(job))
                .Build();
        }

        private static string SerializeJobData(ITaskSchedulerJob job)
        {
            return job.SerializeJobData();
        }

        private static void DeserializeJobData(ITaskSchedulerJob job, string data)
        {
            job.DeserializeJobData(data);
        }

        private static TaskSchedulerContext GetContext()
        {
            return new TaskSchedulerContext();
        }
    }
}
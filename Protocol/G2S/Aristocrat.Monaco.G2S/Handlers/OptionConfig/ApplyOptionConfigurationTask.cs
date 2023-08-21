namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using Monaco.Common.Scheduler;
    using Services;

    /// <summary>
    ///     Implements task that applies changes for option change.
    /// </summary>
    public class ApplyOptionConfigurationTask : ITaskSchedulerJob
    {
        private readonly IConfigurationService _configuration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplyOptionConfigurationTask" /> class.
        /// </summary>
        /// <param name="configuration">Change comm config service.</param>
        public ApplyOptionConfigurationTask(IConfigurationService configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        ///     Gets or sets the configuration identifier for the task.
        /// </summary>
        public long ConfigurationId { get; set; }

        /// <summary>
        ///     Gets or sets the transaction identifier for the task.
        /// </summary>
        public long TransactionId { get; set; }

        /// <inheritdoc />
        public void Execute(TaskSchedulerContext context)
        {
            _configuration.Apply(TransactionId);
        }

        /// <inheritdoc />
        public string SerializeJobData()
        {
            return $"{ConfigurationId};{TransactionId}";
        }

        /// <inheritdoc />
        public void DeserializeJobData(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var parts = data.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                ConfigurationId = long.Parse(parts[0]);
                TransactionId = long.Parse(parts[1]);
            }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="ApplyOptionConfigurationTask" /> class.
        /// </summary>
        /// <param name="configurationId">Configuration id.</param>
        /// <param name="transactionId">Transaction id.</param>
        /// <returns>Returns instance of <c>ApplyOptionConfigChangeTask</c>.</returns>
        public static ApplyOptionConfigurationTask Create(long configurationId, long transactionId)
        {
            // passing null as we just need to define type of the tasks, later task will be instantiated by scheduler with all required dependencies for execution
            return new ApplyOptionConfigurationTask(null)
            {
                ConfigurationId = configurationId,
                TransactionId = transactionId
            };
        }
    }
}
namespace Aristocrat.Monaco.G2S.Handlers.CommConfig
{
    using System;
    using Monaco.Common.Scheduler;
    using Services;

    /// <summary>
    ///     Task used to apply comm config changes
    /// </summary>
    public class ApplyCommConfigurationTask : ITaskSchedulerJob
    {
        private readonly IConfigurationService _configuration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplyCommConfigurationTask" /> class.
        /// </summary>
        /// <param name="configuration">Change comm config service.</param>
        public ApplyCommConfigurationTask(IConfigurationService configuration)
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
        ///     Creates a new instance of the <see cref="ApplyCommConfigurationTask" /> class.
        /// </summary>
        /// <param name="configurationId">The configuration identifier.</param>
        /// <param name="transactionId">The transaction identifier</param>
        /// <returns>Returns instance of <c>ApplyCommConfigChangeTask</c>.</returns>
        public static ApplyCommConfigurationTask Create(long configurationId, long transactionId)
        {
            return new ApplyCommConfigurationTask(null)
            {
                ConfigurationId = configurationId,
                TransactionId = transactionId
            };
        }
    }
}
namespace Aristocrat.Monaco.G2S.Meters
{
    using Monaco.Common.Scheduler;

    /// <summary>
    ///     Implementation of task that sends Meter reports.
    /// </summary>
    public class MeterReportJob : ITaskSchedulerJob
    {
        private readonly IMetersSubscriptionManager _subscription;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterReportJob" /> class.
        /// </summary>
        public MeterReportJob(IMetersSubscriptionManager subscription)
        {
            _subscription = subscription;
        }

        /// <summary>
        ///     Gets or sets meter subscription type.
        /// </summary>
        public long SubscriptionId { get; set; }

        /// <inheritdoc />
        public void Execute(TaskSchedulerContext context)
        {
            _subscription?.HandleMeterReport(SubscriptionId);
        }

        /// <inheritdoc />
        public string SerializeJobData()
        {
            return SubscriptionId.ToString();
        }

        /// <inheritdoc />
        public void DeserializeJobData(string data)
        {
            if (string.IsNullOrEmpty(data) == false)
            {
                SubscriptionId = long.Parse(data);
            }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="MeterReportJob" /> class.
        /// </summary>
        /// <param name="subscriptionId">The subscriptionId identifier.</param>
        /// <returns>Returns instance of <c>MeterReportJob</c>.</returns>
        public static MeterReportJob Create(long subscriptionId)
        {
            return new MeterReportJob(null)
            {
                SubscriptionId = subscriptionId
            };
        }
    }
}
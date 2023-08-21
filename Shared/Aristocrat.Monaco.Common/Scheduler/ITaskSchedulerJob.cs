namespace Aristocrat.Monaco.Common.Scheduler
{
    /// <summary>
    ///     Base interface for task scheduler job.
    /// </summary>
    public interface ITaskSchedulerJob
    {
        /// <summary>
        ///     Executes current job.
        /// </summary>
        /// <param name="context">Current task scheduler context.</param>
        void Execute(TaskSchedulerContext context);

        /// <summary>
        ///     Serializes task scheduler job data.
        /// </summary>
        /// <returns>Returns serialized task scheduler job data.</returns>
        string SerializeJobData();

        /// <summary>
        ///     Deserializes data into existing job.
        /// </summary>
        /// <param name="data">Data to deserialize.</param>
        void DeserializeJobData(string data);
    }
}
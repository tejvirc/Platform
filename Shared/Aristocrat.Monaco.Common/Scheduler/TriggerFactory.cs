namespace Aristocrat.Monaco.Common.Scheduler
{
    using System;
    using Quartz;

    /// <summary>
    ///     Base implementation of Quartz trigger factory.
    /// </summary>
    public static class TriggerFactory
    {
        private const string DefaultTriggerGroupName = "Monaco.Triggers";

        /// <summary>
        ///     Gets trigger that runs task at specified time.
        /// </summary>
        /// <param name="startTime">Exact time when to start task.</param>
        /// <returns>Returns trigger.</returns>
        public static ITrigger GetStartTrigger(DateTime startTime)
        {
            var offset = new DateTimeOffset(startTime);

            return TriggerBuilder.Create()
                .WithIdentity("StartAtTrigger", DefaultTriggerGroupName)
                .StartAt(offset)
                .Build();
        }

        /// <summary>
        ///     Gets trigger that runs task at specified time.
        /// </summary>
        /// <param name="identity">Trigger identity.</param>
        /// <param name="startTime">Exact time when to start task.</param>
        /// <returns>Returns trigger.</returns>
        public static ITrigger GetTrigger(string identity, DateTime startTime)
        {
            var offset = new DateTimeOffset(startTime);

            return TriggerBuilder.Create()
                .WithIdentity(identity, DefaultTriggerGroupName)
                .StartAt(offset)
                .Build();
        }
    }
}
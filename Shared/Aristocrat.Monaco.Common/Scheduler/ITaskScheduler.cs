namespace Aristocrat.Monaco.Common.Scheduler
{
    using System;

    /// <summary>
    ///     Base interface for task scheduler.
    /// </summary>
    public interface ITaskScheduler
    {
        /// <summary>
        ///     Schedules a new task.
        /// </summary>
        /// <param name="job">Task instance.</param>
        /// <param name="startTime">Exact time when to execute task.</param>
        void ScheduleTask(ITaskSchedulerJob job, DateTime startTime);

        /// <summary>
        ///     Schedules a new task.
        /// </summary>
        /// <param name="job">Task instance.</param>
        /// <param name="identity">Job Id.</param>
        /// <param name="startTime">Exact time when to execute task.</param>
        void ScheduleTask(ITaskSchedulerJob job, string identity, DateTime startTime);
    }
}
namespace Aristocrat.Monaco.Common.Tests.Scheduler
{
    using System;
    using Common.Scheduler;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using SimpleInjector;

    [TestClass]
    public class TaskSchedulerTest
    {
        [Ignore]
        [TestMethod]
        public void WhenScheduleTaskExpectSuccess()
        {
            var taskScheduler = new TaskScheduler(new Container());
            taskScheduler
                .ScheduleTask(new Mock<ITaskSchedulerJob>().Object, DateTime.Now);
            taskScheduler
                .ScheduleTask(new Mock<ITaskSchedulerJob>().Object, "Monaco.Triggers", DateTime.Now);
        }

        [Ignore]
        [TestMethod]
        public void WhenDisposeTaskSchedulerExpectSuccess()
        {
            var taskScheduler = new TaskScheduler(new Container());
            taskScheduler
                .ScheduleTask(new Mock<ITaskSchedulerJob>().Object, DateTime.Now);

            taskScheduler.Dispose();
            taskScheduler.Dispose();
        }
    }
}
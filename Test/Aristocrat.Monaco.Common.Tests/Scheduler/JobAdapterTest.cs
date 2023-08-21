namespace Aristocrat.Monaco.Common.Tests.Scheduler
{
    using System.Collections.Generic;
    using Common.Scheduler;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Quartz;
    using SimpleInjector;

    [TestClass]
    public class JobAdapterTest
    {
        private const string JobFullTypeNameKey = "JobFullTypeNameKey";

        private const string TaskSchedulerJobPath = "Aristocrat.Monaco.Common.Scheduler.ITaskSchedulerJob";

        [TestMethod]
        public void WhenExecuteExpectSuccess()
        {
            var container = new Container();
            var taskSchedulerJobMock = new Mock<ITaskSchedulerJob>();
            container.Register(() => taskSchedulerJobMock.Object);

            var jobAdapter = new JobAdapter(container);

            var jobDetailMock = new Mock<IJobDetail>();
            var jobDataMap = new JobDataMap();
            jobDataMap
                .Add(
                    new KeyValuePair<string, object>(
                        JobFullTypeNameKey,
                        TaskSchedulerJobPath));

            jobDetailMock.SetupGet(m => m.JobDataMap).Returns(jobDataMap);

            var contextMock = new Mock<IJobExecutionContext>();
            contextMock.SetupGet(m => m.JobDetail).Returns(jobDetailMock.Object);

            jobAdapter.Execute(contextMock.Object);
        }

        [TestMethod]
        public void WhenConvertJobExpectSuccess()
        {
            var taskScheduler = new Mock<ITaskSchedulerJob>();

            var jobDetail = JobAdapter.ConvertJob(taskScheduler.Object);
            Assert.IsNotNull(jobDetail);
        }
    }
}
namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using G2S.Handlers.OptionConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Scheduler;
    using Moq;

    [TestClass]
    public class ApplyOptionConfigChangeTaskTest
    {
        private const int ConfigurationId = 10;

        private const long TransactionId = 20;

        private Mock<IConfigurationService> _configurationMock;

        [TestInitialize]
        public void Initialize()
        {
            _configurationMock = new Mock<IConfigurationService>();
        }

        [TestMethod]
        public void WhenCreateApplyOptionConfigChangeTaskExpectSuccess()
        {
            var applyOptionConfigChangeTask = ApplyOptionConfigurationTask.Create(ConfigurationId, TransactionId);

            Assert.IsNotNull(applyOptionConfigChangeTask);
            Assert.AreEqual(ConfigurationId, applyOptionConfigChangeTask.ConfigurationId);
            Assert.AreEqual(TransactionId, applyOptionConfigChangeTask.TransactionId);
        }

        [TestMethod]
        public void WhenExecuteExpectSuccess()
        {
            var applyOptionConfigChangeTask = CreateApplyOptionConfigChangeTask(ConfigurationId, TransactionId);

            applyOptionConfigChangeTask.Execute(new TaskSchedulerContext());

            _configurationMock.Verify(m => m.Apply(TransactionId));
        }

        private ApplyOptionConfigurationTask CreateApplyOptionConfigChangeTask(int configurationId, long transactionId)
        {
            return new ApplyOptionConfigurationTask(_configurationMock.Object)
            {
                ConfigurationId = configurationId,
                TransactionId = transactionId
            };
        }
    }
}
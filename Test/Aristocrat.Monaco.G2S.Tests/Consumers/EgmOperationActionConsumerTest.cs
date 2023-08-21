namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
/*
    using System;
    using Data.CommConfig;
    using Data.Model;
    using Data.OptionConfig;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class EgmOperationActionConsumerTest : BaseConsumerTest
    {
        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenChangeLogRepositoryIsNullExpectException()
        {
            var consumer = new EgmOperationConfirmedConsumer(
                this.changeConfigRulesServiceMock.Object,
                this.changeCommConfigServiceMock.Object,
                this.changeOptionConfigServiceMock.Object,
                this.commChangeLogRepositoryMock.Object,
                null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenCommChangeLogIsNullConsumeTest()
        {
            var consumer = CreateConsumer();
            consumer.Consume(new EgmOperatorActionEvent());

            this.commChangeLogRepositoryMock.Verify(x => x.Update(It.IsAny<CommChangeLog>()), Times.Never);
            this.changeConfigRulesServiceMock.Verify(x => x.AllowApply(It.IsAny<CommChangeLog>()), Times.Never);
            this.changeCommConfigServiceMock.Verify(x => x.ApplyChanges(It.IsAny<CommChangeLog>()), Times.Never);
        }

        [TestMethod]
        public void WhenOptionChangeLogIsNullConsumeTest()
        {
            var consumer = CreateConsumer();
            consumer.Consume(new EgmOperatorActionEvent());

            this.optionChangeLogRepositoryMock.Verify(x => x.Update(It.IsAny<OptionChangeLog>()), Times.Never);
            this.changeConfigRulesServiceMock.Verify(x => x.AllowApply(It.IsAny<OptionChangeLog>()), Times.Never);
            this.changeOptionConfigServiceMock.Verify(x => x.ApplyChanges(It.IsAny<OptionChangeLog>()), Times.Never);
        }

        [TestMethod]
        public void WhenCommChangeLogNotNullConsumeTest()
        {
            this.commChangeLogRepositoryMock.Setup(x => x.GetLastPendingChangeLog())
                .Returns(new CommChangeLog(2) { ChangeStatus = ChangeStatus.Pending });

            this.changeConfigRulesServiceMock.Setup(
                    x =>
                        x.AllowApply(
                            It.Is<CommChangeLog>(
                                log =>
                                    (log.Id == 2) && (log.ChangeStatus == ChangeStatus.Pending) &&
                                    log.EgmActionConfirmed)))
                .Returns(true);

            var consumer = CreateConsumer();
            consumer.Consume(new EgmOperatorActionEvent());

            this.commChangeLogRepositoryMock.Verify(
                x =>
                    x.Update(
                        It.Is<CommChangeLog>(
                            log => (log.Id == 2) && (log.ChangeStatus == ChangeStatus.Pending) && log.EgmActionConfirmed)),
                Times.Once);

            this.changeConfigRulesServiceMock.Verify(
                x =>
                    x.AllowApply(
                        It.Is<CommChangeLog>(
                            log => (log.Id == 2) && (log.ChangeStatus == ChangeStatus.Pending) && log.EgmActionConfirmed)),
                Times.Once);

            this.changeCommConfigServiceMock.Verify(
                x =>
                    x.ApplyChanges(
                        It.Is<CommChangeLog>(
                            log => (log.Id == 2) && (log.ChangeStatus == ChangeStatus.Pending) && log.EgmActionConfirmed)),
                Times.Once);
        }

        [TestMethod]
        public void WhenOptionChangeLogNotNullConsumeTest()
        {
            this.optionChangeLogRepositoryMock.Setup(x => x.GetLastPendingChangeLog())
                .Returns(new OptionChangeLog(2) { ChangeStatus = ChangeStatus.Pending });

            this.changeConfigRulesServiceMock.Setup(
                    x =>
                        x.AllowApply(
                            It.Is<OptionChangeLog>(
                                log =>
                                    (log.Id == 2) && (log.ChangeStatus == ChangeStatus.Pending) &&
                                    log.EgmActionConfirmed)))
                .Returns(true);

            var consumer = CreateConsumer();
            consumer.Consume(new EgmOperatorActionEvent());

            this.optionChangeLogRepositoryMock.Verify(
                x =>
                    x.Update(
                        It.Is<OptionChangeLog>(
                            log => (log.Id == 2) && (log.ChangeStatus == ChangeStatus.Pending) && log.EgmActionConfirmed)),
                Times.Once);

            this.changeConfigRulesServiceMock.Verify(
                x =>
                    x.AllowApply(
                        It.Is<OptionChangeLog>(
                            log => (log.Id == 2) && (log.ChangeStatus == ChangeStatus.Pending) && log.EgmActionConfirmed)),
                Times.Once);

            this.changeOptionConfigServiceMock.Verify(
                x =>
                    x.ApplyChanges(
                        It.Is<OptionChangeLog>(
                            log => (log.Id == 2) && (log.ChangeStatus == ChangeStatus.Pending) && log.EgmActionConfirmed)),
                Times.Once);
        }

        private EgmOperationConfirmedConsumer CreateConsumer()
        {
            var consumer = new EgmOperationConfirmedConsumer(
                this.changeConfigRulesServiceMock.Object,
                this.changeCommConfigServiceMock.Object,
                this.changeOptionConfigServiceMock.Object,
                this.commChangeLogRepositoryMock.Object,
                this.optionChangeLogRepositoryMock.Object);

            return consumer;
        }
    }
*/
}
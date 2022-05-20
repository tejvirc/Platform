namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using G2S.Handlers;
    using G2S.Handlers.OptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     <see cref="SetOptionChangeTest" />
    /// </summary>
    [TestClass]
    public class OptionChangeStatusCommandBuilderTest
    {
        private Mock<IMonacoContextFactory> _contextFactoryMock;
        private Mock<IOptionChangeLogRepository> _optionChangeLogRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _optionChangeLogRepositoryMock = new Mock<IOptionChangeLogRepository>();
            _contextFactoryMock = new Mock<IMonacoContextFactory>();
        }

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<OptionChangeStatusCommandBuilder>();
        }

        [TestMethod]
        public void WhenConstructWithAllParametersExpectSuccess()
        {
            var builder = new OptionChangeStatusCommandBuilder(
                _optionChangeLogRepositoryMock.Object,
                _contextFactoryMock.Object);

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        public async Task WhenBuildWithValidParamsExpectSuccess()
        {
            var applyCondition = ApplyCondition.EgmAction;
            var disableCondition = DisableCondition.Immediate;

            var authorizationState = AuthorizationState.Authorized;

            var transactionId = 1;
            _optionChangeLogRepositoryMock.Setup(x => x.GetByTransactionId(It.IsAny<DbContext>(), transactionId))
                .Returns(
                    new OptionChangeLog
                    {
                        TransactionId = transactionId,
                        ApplyCondition = applyCondition,
                        DisableCondition = disableCondition,
                        AuthorizeItems = new List<ConfigChangeAuthorizeItem>
                        {
                            new ConfigChangeAuthorizeItem
                            {
                                AuthorizeStatus = authorizationState,
                                TimeoutDate = DateTime.MaxValue
                            }
                        }
                    });

            var optionConfigDevice = new Mock<IOptionConfigDevice>();

            var builder = new OptionChangeStatusCommandBuilder(
                _optionChangeLogRepositoryMock.Object,
                _contextFactoryMock.Object);

            var command = new optionChangeStatus { transactionId = transactionId };

            await builder.Build(optionConfigDevice.Object, command);

            Assert.AreEqual(command.applyCondition, applyCondition.ToG2SString());
            Assert.AreEqual(command.disableCondition, disableCondition.ToG2SString());

            Assert.AreEqual(command.authorizeStatusList.authorizeStatus.Length, 1);
            Assert.AreEqual(
                command.authorizeStatusList.authorizeStatus.First().authorizationState,
                t_authorizationStates.G2S_authorized);
            Assert.AreEqual(command.authorizeStatusList.authorizeStatus.First().timeoutDate, DateTime.MaxValue);
        }
    }
}
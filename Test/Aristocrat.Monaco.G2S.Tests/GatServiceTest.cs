namespace Aristocrat.Monaco.G2S.Tests.GAT
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Authentication;
    using Aristocrat.G2S.Client;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Storage;
    using Kernel;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class GatServiceTest
    {
        private Mock<IAuthenticationService> _componentHashServiceMock;
        private Mock<IComponentRegistry> _componentRepositoryMock;
        private Mock<IMonacoContextFactory> _contextFactoryMock;
        private Mock<IG2SEgm> _egmMock;
        private Mock<IEventBus> _eventBusMock;
        private Mock<IEventLift> _eventLiftMock;
        private Mock<IGatComponentVerificationRepository> _gatComponentVerificationRepositoryMock;
        private Mock<IGatSpecialFunctionRepository> _gatSpecialFunctionRepositoryMock;
        private Mock<IGatVerificationRequestRepository> _gatVerificationRequestRepositoryMock;
        private Mock<IIdProvider> _idProvider;

        [TestInitialize]
        public void Initialize()
        {
            _componentRepositoryMock = new Mock<IComponentRegistry>();
            _gatVerificationRequestRepositoryMock = new Mock<IGatVerificationRequestRepository>();
            _gatComponentVerificationRepositoryMock = new Mock<IGatComponentVerificationRepository>();
            _gatSpecialFunctionRepositoryMock = new Mock<IGatSpecialFunctionRepository>();
            _componentHashServiceMock = new Mock<IAuthenticationService>();
            _contextFactoryMock = new Mock<IMonacoContextFactory>();
            _idProvider = new Mock<IIdProvider>();
            _eventBusMock = new Mock<IEventBus>();
            _egmMock = new Mock<IG2SEgm>();
            _eventLiftMock = new Mock<IEventLift>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullComponentRepositoryExpectException()
        {
            var service = new GatService(null, null, null, null, null, null, null, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGatVerificationRequestRepositoryExpectException()
        {
            var service = new GatService(
                _componentRepositoryMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGatComponentVerificationRepositoryExpectException()
        {
            var service = new GatService(
                _componentRepositoryMock.Object,
                _gatVerificationRequestRepositoryMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGatSpecialFunctionRepositoryExpectException()
        {
            var service = new GatService(
                _componentRepositoryMock.Object,
                _gatVerificationRequestRepositoryMock.Object,
                _gatComponentVerificationRepositoryMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullComponentHashServiceExpectException()
        {
            var service = new GatService(
                _componentRepositoryMock.Object,
                _gatVerificationRequestRepositoryMock.Object,
                _gatComponentVerificationRepositoryMock.Object,
                _gatSpecialFunctionRepositoryMock.Object,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryExpectException()
        {
            var service = new GatService(
                _componentRepositoryMock.Object,
                _gatVerificationRequestRepositoryMock.Object,
                _gatComponentVerificationRepositoryMock.Object,
                _gatSpecialFunctionRepositoryMock.Object,
                _componentHashServiceMock.Object,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullDeviceRegistryServiceExpectException()
        {
            var service = new GatService(
                _componentRepositoryMock.Object,
                _gatVerificationRequestRepositoryMock.Object,
                _gatComponentVerificationRepositoryMock.Object,
                _gatSpecialFunctionRepositoryMock.Object,
                _componentHashServiceMock.Object,
                _contextFactoryMock.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        public void WhenHasVerificationIdExpectTrue()
        {
            var verificationId = 10;

            var gatVerificationRequest = new GatVerificationRequest
            {
                VerificationId = verificationId
            };

            _gatVerificationRequestRepositoryMock.Setup(
                    m => m.GetByVerificationId(It.IsAny<DbContext>(), verificationId))
                .Returns(gatVerificationRequest);

            var service = CreateService();

            var actual = service.HasVerificationId(verificationId);

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void WhenHasVerificationIdExpectFalse()
        {
            var verificationId = 10;

            var gatVerificationRequest = new GatVerificationRequest
            {
                VerificationId = verificationId
            };

            _gatVerificationRequestRepositoryMock.Setup(
                    m => m.GetByVerificationId(It.IsAny<DbContext>(), verificationId + 1))
                .Returns(gatVerificationRequest);

            var service = CreateService();

            var actual = service.HasVerificationId(verificationId);

            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        public void WhenHasTransactionIdExpectTrue()
        {
            var transactionId = 10;

            var gatVerificationRequest = new GatVerificationRequest
            {
                TransactionId = transactionId
            };

            _gatVerificationRequestRepositoryMock.Setup(
                    m => m.GetByTransactionId(It.IsAny<DbContext>(), transactionId))
                .Returns(gatVerificationRequest);

            var service = CreateService();

            var actual = service.HasTransactionId(transactionId);

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void WhenHasTransactionIdExpectFalse()
        {
            var transactionId = 10;

            var gatVerificationRequest = new GatVerificationRequest
            {
                TransactionId = transactionId
            };

            _gatVerificationRequestRepositoryMock.Setup(
                    m => m.GetByTransactionId(It.IsAny<DbContext>(), transactionId + 1))
                .Returns(gatVerificationRequest);

            var service = CreateService();

            var actual = service.HasTransactionId(transactionId);

            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        public void WhenGetVerificationRequestByIdExpectSuccess()
        {
            var verificationId = 10;

            var gatVerificationRequest = new GatVerificationRequest
            {
                VerificationId = verificationId
            };

            _gatVerificationRequestRepositoryMock.Setup(
                    m => m.GetByVerificationId(It.IsAny<DbContext>(), verificationId))
                .Returns(gatVerificationRequest);

            var service = CreateService();

            var result = service.GetVerificationRequestById(verificationId);

            Assert.AreEqual(verificationId, result.VerificationId);
        }

        [TestMethod]
        public void WhenGetComponentListExpectNoException()
        {
            var service = CreateService();
            service.GetComponentList();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDoVerificationWithNullVerificationArgsExpectException()
        {
            var service = CreateService();
            service.DoVerification(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGetVerificationStatusWithNullGetVerificationStatusByTransactionArgsExpectException()
        {
            var service = CreateService();
            service.GetVerificationStatus((GetVerificationStatusByTransactionArgs)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGetVerificationStatusWithNullGetVerificationStatusByDeviceArgsExpectException()
        {
            var service = CreateService();
            service.GetVerificationStatus((GetVerificationStatusByDeviceArgs)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSaveVerificationAckWithNullSaveVerificationAckArgsExpectException()
        {
            var service = CreateService();
            service.SaveVerificationAck(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSaveComponentWithNullComponentExpectException()
        {
            var service = CreateService();
            service.SaveComponent(null);
        }

        [TestMethod]
        public void WhenGetSpecialFunctionsExpectNoException()
        {
            var service = CreateService();
            service.GetSpecialFunctions();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSaveSpecialFunctionsWithNullGatSpecialFunctionExpectException()
        {
            var service = CreateService();
            service.SaveSpecialFunction(null);
        }

        [TestMethod]
        public void WhenGetLogForTransactionIdExpectSuccess()
        {
            var transactionId = 10;
            var verificationId = 15;

            var gatVerificationRequest = new GatVerificationRequest
            {
                TransactionId = transactionId,
                VerificationId = verificationId,
                ComponentVerifications = new List<GatComponentVerification>
                {
                    new GatComponentVerification
                    {
                        RequestId = verificationId
                    }
                }
            };

            _gatVerificationRequestRepositoryMock.Setup(m => m.GetByTransactionId(It.IsAny<DbContext>(), transactionId))
                .Returns(gatVerificationRequest);

            var service = CreateService();
            var result = service.GetLogForTransactionId(transactionId);

            Assert.AreEqual(transactionId, result.TransactionId);
            Assert.AreEqual(verificationId, result.VerificationId);

            Assert.AreEqual(1, result.ComponentVerifications.Count);
            Assert.AreEqual(verificationId, result.ComponentVerifications.First().RequestId);
        }

        [TestMethod]
        public void WhenGetSupportedAlgorithmsExpectSuccess()
        {
            var serivce = CreateService();
            serivce.GetSupportedAlgorithms(ComponentType.None);
        }

        [TestMethod]
        public void WhenGetLogStatusExpectSuccess()
        {
            var serivce = CreateService();
            serivce.GetLogStatus();
        }

        [TestMethod]
        public void WhenDeleteComponentExpectSuccess()
        {
            var componentId = "AAA712AA";
            var componentType = ComponentType.Hardware;

            var serivce = CreateService();
            serivce.DeleteComponent(componentId, componentType);
            _componentRepositoryMock.Verify(
                m => m.UnRegister(componentId, false),
                Times.Once);
        }

        [TestMethod]
        public void WhenGetGatComponentVerificationEntityExpectSuccess()
        {
            var componentId = "AAA712AA";
            var verificationId = 15;

            var gatComponentVerification = new GatComponentVerification
            {
                ComponentId = componentId,
                RequestId = verificationId
            };

            _gatVerificationRequestRepositoryMock
                .Setup(m => m.GetByVerificationId(It.IsAny<DbContext>(), verificationId))
                .Returns(new GatVerificationRequest(verificationId));

            _gatComponentVerificationRepositoryMock
                .Setup(m => m.GetByComponentIdAndVerificationId(It.IsAny<DbContext>(), componentId, verificationId))
                .Returns(gatComponentVerification);

            var service = CreateService();
            var result = service.GetGatComponentVerificationEntity(componentId, verificationId);

            Assert.AreEqual(componentId, result.ComponentId);
            Assert.AreEqual(verificationId, result.RequestId);
        }

        [TestMethod]
        public void WhenUpdateGatComponentVerificationEntityExpectSuccess()
        {
            var gatComponentVerification = new GatComponentVerification();

            var service = CreateService();
            service.UpdateGatComponentVerificationEntity(gatComponentVerification);

            _gatComponentVerificationRepositoryMock
                .Verify(m => m.Update(It.IsAny<DbContext>(), gatComponentVerification), Times.Once);
        }

        private GatService CreateService()
        {
            var service = new GatService(
                _componentRepositoryMock.Object,
                _gatVerificationRequestRepositoryMock.Object,
                _gatComponentVerificationRepositoryMock.Object,
                _gatSpecialFunctionRepositoryMock.Object,
                _componentHashServiceMock.Object,
                _contextFactoryMock.Object,
                _idProvider.Object,
                _eventBusMock.Object,
                _egmMock.Object,
                _eventLiftMock.Object);

            return service;
        }
    }
}

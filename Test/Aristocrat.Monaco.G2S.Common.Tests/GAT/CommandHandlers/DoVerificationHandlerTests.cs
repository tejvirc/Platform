namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using Application.Contracts;
    using Application.Contracts.Authentication;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Exceptions;
    using Common.GAT.Storage;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class DoVerificationHandlerTests
    {
        private readonly Mock<IComponentRegistry> _componentRegistry =
            new Mock<IComponentRegistry>();

        private readonly Mock<IAuthenticationService> _componentHashServiceMock =
            new Mock<IAuthenticationService>();

        private readonly Mock<IGatComponentVerificationRepository> _componentVerificationRepositoryMock =
            new Mock<IGatComponentVerificationRepository>();

        private readonly Mock<IMonacoContextFactory> _contextFactoryMock = new Mock<IMonacoContextFactory>();

        private readonly Mock<IIdProvider> _idProvider = new Mock<IIdProvider>();

        private readonly Mock<IGatVerificationRequestRepository> _verificationRequestRepositoryMock =
            new Mock<IGatVerificationRequestRepository>();

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfVerificationRequestRepositoryIsNull()
        {
            new DoVerificationHandler(null, null, null, null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfComponentVerificationRepositoryIsNull()
        {
            new DoVerificationHandler(
                _verificationRequestRepositoryMock.Object,
                null,
                null,
                null,
                null,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfComponentRepositoryIsNull()
        {
            new DoVerificationHandler(
                _verificationRequestRepositoryMock.Object,
                _componentVerificationRepositoryMock.Object,
                null,
                null,
                null,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfComponentHashServiceIsNull()
        {
            new DoVerificationHandler(
                _verificationRequestRepositoryMock.Object,
                _componentVerificationRepositoryMock.Object,
                _componentRegistry.Object,
                null,
                null,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfComponentDeviceRegistryServiceIsNull()
        {
            new DoVerificationHandler(
                _verificationRequestRepositoryMock.Object,
                _componentVerificationRepositoryMock.Object,
                _componentRegistry.Object,
                _componentHashServiceMock.Object,
                null,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfContextFactoryIsNull()
        {
            new DoVerificationHandler(
                _verificationRequestRepositoryMock.Object,
                _componentVerificationRepositoryMock.Object,
                _componentRegistry.Object,
                _componentHashServiceMock.Object,
                null,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfInitVerificationArgsIsNull()
        {
            var handler = new DoVerificationHandler(
                _verificationRequestRepositoryMock.Object,
                _componentVerificationRepositoryMock.Object,
                _componentRegistry.Object,
                _componentHashServiceMock.Object,
                _contextFactoryMock.Object,
                _idProvider.Object);

            handler.Execute(null);
        }

        [TestMethod]
        [ExpectedException(typeof(DeviceIdNotCorrespondingVerificationIdException))]
        public void ThrowIfDuplicationVerificationId()
        {
            var data = new GatVerificationRequest(1)
            {
                DeviceId = 5,
                VerificationId = 1,
                ComponentVerifications = new List<GatComponentVerification>()
            };

            _verificationRequestRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<GatVerificationRequest, bool>>>()))
                .Returns(new List<GatVerificationRequest>().AsQueryable());
            _verificationRequestRepositoryMock
                .Setup(x => x.GetByVerificationId(It.IsAny<DbContext>(), It.IsAny<long>()))
                .Returns(data);

            var handler = new DoVerificationHandler(
                _verificationRequestRepositoryMock.Object,
                _componentVerificationRepositoryMock.Object,
                _componentRegistry.Object,
                _componentHashServiceMock.Object,
                _contextFactoryMock.Object,
                _idProvider.Object);

            handler.Execute(
                new DoVerificationArgs(
                    1,
                    1,
                    "empl",
                    new List<VerifyComponent>
                    {
                        new VerifyComponent(
                            "comp_1",
                            AlgorithmType.Crc16,
                            "seed",
                            "salt",
                            1,
                            2)
                    }));
        }

        [TestMethod]
        public void TestInCaseVerificationExistsByDeviceIdAndVerificationId()
        {
            var list = new List<GatVerificationRequest>
            {
                new GatVerificationRequest(1)
                {
                    DeviceId = 1,
                    VerificationId = 1,
                    ComponentVerifications = new List<GatComponentVerification>()
                }
            };
            _verificationRequestRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<GatVerificationRequest, bool>>>()))
                .Returns(
                    (DbContext contextx, Expression<Func<GatVerificationRequest, bool>> predicate) =>
                        list.AsQueryable());

            var handler = new DoVerificationHandler(
                _verificationRequestRepositoryMock.Object,
                _componentVerificationRepositoryMock.Object,
                _componentRegistry.Object,
                _componentHashServiceMock.Object,
                _contextFactoryMock.Object,
                _idProvider.Object);

            var result = handler.Execute(
                new DoVerificationArgs(
                    1,
                    1,
                    "empl",
                    new List<VerifyComponent>
                    {
                        new VerifyComponent(
                            "comp_1",
                            AlgorithmType.Crc16,
                            "seed",
                            "salt",
                            1,
                            2)
                    }));

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SaveComponentVerificationRequestEntityTest()
        {
            _componentRegistry.SetupGet(m => m.Components).Returns(
                new List<Component>
                {
                    new Component { ComponentId = "comp_1" },
                    new Component { ComponentId = "comp_2" }
                });

            _verificationRequestRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<GatVerificationRequest, bool>>>()))
                .Returns(
                    (DbContext context, Expression<Func<GatVerificationRequest, bool>> predicate) =>
                        new List<GatVerificationRequest>().AsQueryable());

            _idProvider.Setup(m => m.GetNextTransactionId()).Returns(1);

            var handler = new DoVerificationHandler(
                _verificationRequestRepositoryMock.Object,
                _componentVerificationRepositoryMock.Object,
                _componentRegistry.Object,
                _componentHashServiceMock.Object,
                _contextFactoryMock.Object,
                _idProvider.Object);

            long verificationId = 1;

            var result = handler.Execute(
                new DoVerificationArgs(
                    verificationId,
                    1,
                    "empl",
                    new List<VerifyComponent>
                    {
                        new VerifyComponent(
                            "comp_1",
                            AlgorithmType.Crc16,
                            ConvertExtensions.ToPackedHexString(Encoding.ASCII.GetBytes("seed")),
                            "salt",
                            1,
                            2),
                        new VerifyComponent(
                            "comp_2",
                            AlgorithmType.Crc32,
                            ConvertExtensions.ToPackedHexString(Encoding.ASCII.GetBytes("seed")),
                            "salt",
                            3,
                            4)
                    }));

            _verificationRequestRepositoryMock.Verify(
                x => x.Add(It.IsAny<DbContext>(), It.IsAny<GatVerificationRequest>()),
                Times.Once);
            Assert.IsTrue(
                (result.TransactionId == 1) && (result.ComponentStatuses.Count() == 2) &&
                (result.VerificationId == verificationId));
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownComponentException))]
        public void ThrowIfComponentNotExixtsInDbTest()
        {
            _componentRegistry.SetupGet(m => m.Components).Returns(new List<Component> { new Component() });

            _verificationRequestRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<GatVerificationRequest, bool>>>()))
                .Returns(
                    (DbContext context, Expression<Func<GatVerificationRequest, bool>> predicate) =>
                        new List<GatVerificationRequest>().AsQueryable());

            var handler = new DoVerificationHandler(
                _verificationRequestRepositoryMock.Object,
                _componentVerificationRepositoryMock.Object,
                _componentRegistry.Object,
                _componentHashServiceMock.Object,
                _contextFactoryMock.Object,
                _idProvider.Object);

            handler.Execute(
                new DoVerificationArgs(
                    1,
                    1,
                    "empl",
                    new List<VerifyComponent>
                    {
                        new VerifyComponent(
                            "comp_1",
                            AlgorithmType.Crc16,
                            "seed",
                            "salt",
                            1,
                            2)
                    }));
        }
    }
}

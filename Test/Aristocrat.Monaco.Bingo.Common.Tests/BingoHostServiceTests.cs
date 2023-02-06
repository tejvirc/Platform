namespace Aristocrat.Monaco.Bingo.Common.Tests
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Storage.Model;
    using Aristocrat.Monaco.Protocol.Common.Storage;

    [TestClass]
    public class BingoHostServiceTests
    {
        private BingoHostService _target;
        private readonly Mock<IMonacoContextFactory> _factory = new Mock<IMonacoContextFactory>(MockBehavior.Default);
        private readonly Mock<IRepository<Host>> _repository = new Mock<IRepository<Host>>(MockBehavior.Default);
        private string defaultHostName = string.Empty;
        private const int defaultPort = 443;

        [TestMethod]
        public void Constructor()
        {
            _target = new BingoHostService(_factory.Object, _repository.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTest()
        {
            _target = new BingoHostService(_factory.Object, null);
        }

        [TestMethod]
        public void NewHostTest()
        {
            _target = new BingoHostService(_factory.Object, _repository.Object);

            var oldHost = CreateHost("oldHost", 1, 123);
            var newHost = CreateHost("newHost", 1, 1234);

            var mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
            mockConnectionStringResolver.Setup(a => a.Resolve()).Returns("TestDb");
            var dbContext = new BingoContext(mockConnectionStringResolver.Object);

            _factory.Setup(x => x.CreateDbContext()).Returns(dbContext).Verifiable();
            _repository.Setup(r => r.GetSingle(dbContext)).Returns(oldHost).Verifiable();
            _repository.Setup(r => r.Delete(dbContext, oldHost)).Verifiable();
            _repository.Setup(r => r.Add(dbContext, newHost)).Verifiable();

            _target.SaveHost(newHost);

            _repository.Setup(r => r.GetSingle(dbContext)).Returns(newHost).Verifiable();

            var currentHost = _target.GetHost();

            _factory.Verify();
            _repository.Verify();

            Assert.AreEqual(newHost, currentHost);
        }

        [TestMethod]
        public void UpdatedHostTest()
        {
            _target = new BingoHostService(_factory.Object, _repository.Object);

            var oldHost = CreateHost("oldHost", 1, 123);
            var updatedOldHost = CreateHost("UpdatedHost", 2, 123);

            var mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
            mockConnectionStringResolver.Setup(a => a.Resolve()).Returns("TestDb");

            var dbContext = new BingoContext(mockConnectionStringResolver.Object);

            _factory.Setup(x => x.CreateDbContext()).Returns(dbContext).Verifiable();
            _repository.Setup(r => r.GetSingle(dbContext)).Returns(oldHost).Verifiable();
            _repository.Setup(r => r.Update(dbContext, oldHost)).Verifiable();

            _target.SaveHost(updatedOldHost);

            var currentHost = _target.GetHost();

            _factory.Verify();
            _repository.Verify();

            Assert.AreEqual(updatedOldHost.HostName, currentHost.HostName);
            Assert.AreEqual(updatedOldHost.Port, currentHost.Port);
            Assert.AreEqual(oldHost, currentHost);
        }

        [TestMethod]
        public void NullHostTest()
        {
            _target = new BingoHostService(_factory.Object, _repository.Object);

            var newHost = CreateHost("newHost", 2, 123);

            var mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
            mockConnectionStringResolver.Setup(a => a.Resolve()).Returns("TestDb");
            var dbContext = new BingoContext(mockConnectionStringResolver.Object);

            _factory.Setup(x => x.CreateDbContext()).Returns(dbContext).Verifiable();
            _repository.Setup(r => r.GetSingle(dbContext)).Returns((Host)null).Verifiable();
            _repository.Setup(r => r.Add(dbContext, newHost)).Verifiable();

            _target.SaveHost(newHost);

            _repository.Setup(r => r.GetSingle(dbContext)).Returns(newHost).Verifiable();

            var currentHost = _target.GetHost();

            _factory.Verify();
            _repository.Verify();

            Assert.AreEqual(newHost, currentHost);
        }

        [TestMethod]
        public void DefaultHostTest()
        {
            _target = new BingoHostService(_factory.Object, _repository.Object);

            var mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
            mockConnectionStringResolver.Setup(a => a.Resolve()).Returns("TestDb");
            var dbContext = new BingoContext(mockConnectionStringResolver.Object);

            _factory.Setup(x => x.CreateDbContext()).Returns(dbContext).Verifiable();
            _repository.Setup(r => r.GetSingle(dbContext)).Returns((Host)null).Verifiable();

            var currentHost = _target.GetHost();

            _factory.Verify();
            _repository.Verify();

            Assert.AreEqual(defaultHostName, currentHost.HostName);
            Assert.AreEqual(defaultPort, currentHost.Port);
        }

        private Host CreateHost(string hostName, int port, long id)
        {
            return new Host {HostName = hostName, Id = id, Port = port};
        }
    }
}

namespace Aristocrat.Monaco.G2S.Data.Tests
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Common.Storage;
    using Data.Model;
    using Hosts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HostServiceTest
    {
        private Mock<IMonacoContextFactory> _contextFactoryMock;

        private Mock<IHostRepository> _hostRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _contextFactoryMock = new Mock<IMonacoContextFactory>();
            _hostRepositoryMock = new Mock<IHostRepository>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryExpectException()
        {
            var hostSerivce = new HostService(null, _hostRepositoryMock.Object);

            Assert.IsNull(hostSerivce);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullHostRepositoryExpectException()
        {
            var hostSerivce = new HostService(_contextFactoryMock.Object, null);

            Assert.IsNull(hostSerivce);
        }

        [TestMethod]
        public void WhenGetAllHostsExpectSuccess()
        {
            var hosts = new[] { new Host(), new Host() };
            _hostRepositoryMock.Setup(m => m.GetAll(It.IsAny<DbContext>()))
                .Returns(hosts.AsQueryable());

            var hostService = CreateHostService();

            Assert.AreEqual(2, hostService.GetAll().Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSaveHostsIsNullExpectException()
        {
            var hostService = CreateHostService();

            hostService.Save(null);
        }

        [TestMethod]
        public void WhenSaveHostsWithDeletedAddedUpdatedExpectException()
        {
            var deleted = new Host { Id = 1 };
            var updated = new Host { Id = 2 };
            var added = new Host { Id = 3 };

            var hosts = new[]
            {
                deleted,
                updated
            };
            _hostRepositoryMock.Setup(m => m.GetAll(It.IsAny<DbContext>()))
                .Returns(hosts.AsQueryable());

            var hostsToSave = new[]
            {
                updated,
                added
            }.AsEnumerable();

            var hostService = CreateHostService();

            hostService.Save(hostsToSave);

            _hostRepositoryMock.Verify(m => m.Add(It.IsAny<DbContext>(), added), Times.Once);
            _hostRepositoryMock.Verify(m => m.Update(It.IsAny<DbContext>(), updated), Times.Once);
            _hostRepositoryMock.Verify(m => m.Delete(It.IsAny<DbContext>(), deleted), Times.Once);
        }

        private HostService CreateHostService()
        {
            return new HostService(
                _contextFactoryMock.Object,
                _hostRepositoryMock.Object);
        }
    }
}
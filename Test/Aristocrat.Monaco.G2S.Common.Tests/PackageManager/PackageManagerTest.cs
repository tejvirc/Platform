namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading;
    using Application.Contracts.Authentication;
    using Common.PackageManager;
    using Common.PackageManager.Storage;
    using Common.Transfer;
    using Data.Model;
    using Data.Packages;
    using Protocol.Common.Installer;
    using Kernel;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class PackageManagerTest
    {
        private Mock<IMonacoContextFactory> _contextFactoryMock;
        private Mock<IAuthenticationService> _hashCalculator;
        private Mock<IInstallerService> _installerServiceMock;
        private Mock<IModuleRepository> _moduleRepositoryMock;
        private Mock<IPackageErrorRepository> _packageErrorRepositoryMock;
        private Mock<IPackageRepository> _packageRepositoryMock;
        private Mock<IPackageLogRepository> _packageLogRepositoryMock;
        private Mock<IPathMapper> _pathMapperMock;
        private Mock<IScriptRepository> _scriptRepositoryMock;
        private Mock<ITransferRepository> _transferRepositoryMock;
        private Mock<ITransferService> _transferSerivceMock;
        private Mock<IComponentRegistry> _componentRegisteryMock;
        private Mock<IFileSystemProvider> _fileSystemProviderMock;

        [TestInitialize]
        public void Initialize()
        {
            _contextFactoryMock = new Mock<IMonacoContextFactory>();
            _packageRepositoryMock = new Mock<IPackageRepository>();
            _packageLogRepositoryMock = new Mock<IPackageLogRepository>();
            _transferRepositoryMock = new Mock<ITransferRepository>();
            _transferSerivceMock = new Mock<ITransferService>();
            _pathMapperMock = new Mock<IPathMapper>();
            _packageErrorRepositoryMock = new Mock<IPackageErrorRepository>();
            _scriptRepositoryMock = new Mock<IScriptRepository>();
            _moduleRepositoryMock = new Mock<IModuleRepository>();

            _installerServiceMock = new Mock<IInstallerService>();
            _hashCalculator = new Mock<IAuthenticationService>();
            _fileSystemProviderMock = new Mock<IFileSystemProvider>();
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _componentRegisteryMock = new Mock<IComponentRegistry>();
        }

        [TestMethod]
        public void PackageAbortTokensPropertyTest()
        {
            var manager = CreatePackageManager();

            Assert.AreEqual(
                new Dictionary<string, CancellationTokenSource>().Count,
                manager.PackageTaskAbortTokens.Count);
        }

        [TestMethod]
        public void PackageCountPropertyTest()
        {
            var count = 15;
            _packageRepositoryMock.Setup(m => m.Count(It.IsAny<DbContext>())).Returns(count);

            var manager = CreatePackageManager();

            Assert.AreEqual(count, manager.PackageCount);
        }

        [TestMethod]
        public void ScriptCountPropertyTest()
        {
            var count = 15;
            _scriptRepositoryMock.Setup(m => m.Count(It.IsAny<DbContext>())).Returns(count);

            var manager = CreatePackageManager();

            Assert.AreEqual(count, manager.ScriptCount);
        }

        [TestMethod]
        public void PackageEntityListPropertyTest()
        {
            var packages = new List<Common.PackageManager.Storage.Package>
            {
                new Common.PackageManager.Storage.Package(),
                new Common.PackageManager.Storage.Package()
            };

            _packageRepositoryMock.Setup(m => m.GetAll(It.IsAny<DbContext>()))
                .Returns(packages.AsQueryable());

            var manager = CreatePackageManager();

            Assert.AreEqual(packages.Count, manager.PackageEntityList.Count);

            _packageRepositoryMock.Verify(m => m.GetAll(It.IsAny<DbContext>()));
        }

        [TestMethod]
        public void ScriptEntityListPropertyTest()
        {
            var scripts = new List<Script>
            {
                new Script(),
                new Script()
            };

            _scriptRepositoryMock.Setup(m => m.GetAll(It.IsAny<DbContext>()))
                .Returns(scripts.AsQueryable());

            var manager = CreatePackageManager();

            Assert.AreEqual(scripts.Count, manager.ScriptEntityList.Count);

            _scriptRepositoryMock.Verify(m => m.GetAll(It.IsAny<DbContext>()));
        }

        [TestMethod]
        public void ModuleEntityListPropertyTest()
        {
            var modules = new List<Module>
            {
                new Module(),
                new Module()
            };

            _moduleRepositoryMock.Setup(m => m.GetAll(It.IsAny<DbContext>()))
                .Returns(modules.AsQueryable());

            var manager = CreatePackageManager();

            Assert.AreEqual(modules.Count, manager.ModuleEntityList.Count);

            _moduleRepositoryMock.Verify(m => m.GetAll(It.IsAny<DbContext>()));
        }

        [TestMethod]
        public void GetPackageEntityTest()
        {
            var packageId = "packageId";
            _packageRepositoryMock
                .Setup(m => m.GetPackageByPackageId(It.IsAny<DbContext>(), packageId))
                .Returns(new Common.PackageManager.Storage.Package { PackageId = packageId });

            var manager = CreatePackageManager();

            var entity = manager.GetPackageEntity(packageId);

            Assert.AreEqual(packageId, entity.PackageId);

            _packageRepositoryMock
                .Verify(m => m.GetPackageByPackageId(It.IsAny<DbContext>(), packageId));
        }

        [TestMethod]
        public void GetScriptTest()
        {
            var scriptId = 12;
            _scriptRepositoryMock
                .Setup(m => m.GetScriptByScriptId(It.IsAny<DbContext>(), scriptId))
                .Returns(new Script { ScriptId = scriptId });

            var manager = CreatePackageManager();

            var entity = manager.GetScript(scriptId);

            Assert.AreEqual(scriptId, entity.ScriptId);

            _scriptRepositoryMock
                .Verify(m => m.GetScriptByScriptId(It.IsAny<DbContext>(), scriptId));
        }

        [TestMethod]
        public void GetModuleEntityTest()
        {
            var moduleId = "moduleId";
            _moduleRepositoryMock
                .Setup(m => m.GetModuleByModuleId(It.IsAny<DbContext>(), moduleId))
                .Returns(new Module { ModuleId = moduleId });

            var manager = CreatePackageManager();

            var entity = manager.GetModuleEntity(moduleId);

            Assert.AreEqual(moduleId, entity.ModuleId);

            _moduleRepositoryMock
                .Verify(m => m.GetModuleByModuleId(It.IsAny<DbContext>(), moduleId));
        }

        [TestMethod]
        public void GetTransferEntityTest()
        {
            var packageId = "packageId";
            _transferRepositoryMock
                .Setup(m => m.GetByPackageId(It.IsAny<DbContext>(), packageId))
                .Returns(new TransferEntity { PackageId = packageId });

            var manager = CreatePackageManager(false);

            var entity = manager.GetTransferEntity(packageId);

            Assert.AreEqual(packageId, entity.PackageId);

            _transferRepositoryMock
                .Verify(m => m.GetByPackageId(It.IsAny<DbContext>(), packageId));
        }

        [TestMethod]
        public void WhenUpdatePackagePackageNotExistsExpectAdd()
        {
            _packageRepositoryMock
                .Setup(m => m.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns((Common.PackageManager.Storage.Package)null);
            var entity = new PackageLog();

            _packageLogRepositoryMock
                .Setup(m => m.GetLastPackageLogeByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns(entity);

            var manager = CreatePackageManager();

            manager.UpdatePackage(entity);

            _packageRepositoryMock.Verify(m => m.Add(It.IsAny<DbContext>(), It.IsAny<Common.PackageManager.Storage.Package>()));
        }

        [TestMethod]
        public void WhenAddPackageLogPackageNotExistsExpectAdd()
        {
            var entity = new PackageLog();

            var manager = CreatePackageManager();

            manager.AddPackageLog(entity);

            _packageLogRepositoryMock.Verify(m => m.Add(It.IsAny<DbContext>(), entity));
        }

        [TestMethod]
        public void WhenUpdatePackagePackageExistsExpectUpdate()
        {
            var entity = new Common.PackageManager.Storage.Package
            {
                PackageId = "packageId"
            };

            var logEntity = new PackageLog
            {
                PackageId = "packageId"
            };

            _packageLogRepositoryMock
                .Setup(m => m.GetLastPackageLogeByPackageId(It.IsAny<DbContext>(), logEntity.PackageId))
                .Returns(logEntity);

            _packageRepositoryMock
                .Setup(m => m.GetPackageByPackageId(It.IsAny<DbContext>(), entity.PackageId))
                .Returns(entity);

            var manager = CreatePackageManager();

            manager.UpdatePackage(logEntity);

            _packageRepositoryMock.Verify(m => m.Update(It.IsAny<DbContext>(), entity));
        }

        [TestMethod]
        public void WhenUpdateModuleModuleNotExistsExpectAdd()
        {
            _moduleRepositoryMock
                .Setup(m => m.GetModuleByModuleId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns((Module)null);
            var entity = new Module();

            var manager = CreatePackageManager();

            manager.UpdateModuleEntity(entity);

            _moduleRepositoryMock.Verify(m => m.Add(It.IsAny<DbContext>(), entity));
        }

        [TestMethod]
        public void WhenUpdateModuleModuleExistsExpectUpdate()
        {
            var moduleToUpdate = new Module { ModuleId = "1" };

            _moduleRepositoryMock
                .Setup(m => m.GetModuleByModuleId(It.IsAny<DbContext>(), moduleToUpdate.ModuleId))
                .Returns(moduleToUpdate);

            var manager = CreatePackageManager();

            var module = new Module
            {
                ModuleId = moduleToUpdate.ModuleId,
                PackageId = "packageId",
                Status = "status"
            };

            manager.UpdateModuleEntity(module);

            Assert.AreEqual(module.Status, moduleToUpdate.Status);
            Assert.AreEqual(module.PackageId, moduleToUpdate.PackageId);

            _moduleRepositoryMock.Verify(m => m.Update(It.IsAny<DbContext>(), moduleToUpdate));
        }

        [TestMethod]
        public void WhenUpdatePackageLogPackageLogNotExistsExpectAdd()
        {
            var logEntity = new PackageLog
            {
                PackageId = "packageId"
            };

            _packageLogRepositoryMock
                .Setup(m => m.GetLastPackageLogeByPackageId(It.IsAny<DbContext>(), logEntity.PackageId))
                .Returns(logEntity);

            _packageRepositoryMock
                .Setup(m => m.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns((Common.PackageManager.Storage.Package)null);
            var entity = new Common.PackageManager.Storage.Package();

            var manager = CreatePackageManager();

            manager.UpdatePackage(logEntity);

            _packageRepositoryMock.Verify(m => m.Add(It.IsAny<DbContext>(), It.IsAny<Common.PackageManager.Storage.Package>()));
        }

        [TestMethod]
        public void WhenUpdatePackageLogPackageLogExistsExpectUpdate()
        {
            var packageLogToUpdate = new Common.PackageManager.Storage.Package { PackageId = "1" };

            _packageRepositoryMock
                .Setup(m => m.GetPackageByPackageId(It.IsAny<DbContext>(), packageLogToUpdate.PackageId))
                .Returns(packageLogToUpdate);

            var packageLog = new PackageLog()
            {
                PackageId = packageLogToUpdate.PackageId,
                DeviceId = 1,
                Exception = 2,
                Id = 3,
                State = (PackageState)2000,
                TransactionId = 4
            };

            _packageLogRepositoryMock
                .Setup(m => m.GetLastPackageLogeByPackageId(It.IsAny<DbContext>(), packageLog.PackageId))
                .Returns(packageLog);

            var manager = CreatePackageManager();


            manager.UpdatePackage(packageLog);

            Assert.AreEqual(packageLog.PackageId, packageLogToUpdate.PackageId);
            Assert.AreEqual(packageLog.State, packageLogToUpdate.State);

            _packageRepositoryMock.Verify(m => m.Update(It.IsAny<DbContext>(), packageLogToUpdate));
        }

        [TestMethod]
        public void WhenUpdateScriptScriptNotExistsExpectAdd()
        {
            _scriptRepositoryMock
                .Setup(m => m.GetScriptByScriptId(It.IsAny<DbContext>(), It.IsAny<int>()))
                .Returns((Script)null);

            var entity = new Script();

            var manager = CreatePackageManager();

            manager.UpdateScript(entity);

            _scriptRepositoryMock.Verify(m => m.Add(It.IsAny<DbContext>(), entity));
        }

        [TestMethod]
        public void WhenUpdateScriptScriptExistsExpectUpdate()
        {
            var scriptToUpdate = new Script { ScriptId = 1 };

            _scriptRepositoryMock
                .Setup(m => m.GetScriptByScriptId(It.IsAny<DbContext>(), scriptToUpdate.ScriptId))
                .Returns(scriptToUpdate);

            var manager = CreatePackageManager();

            var script = new Script { ScriptId = scriptToUpdate.ScriptId };

            manager.UpdateScript(script);

            _scriptRepositoryMock.Verify(m => m.Update(It.IsAny<DbContext>(), scriptToUpdate));
        }

        [TestMethod]
        public void UpdateTransferEntityTest()
        {
            var entity = new TransferEntity();

            var manager = CreatePackageManager();

            manager.UpdateTransferEntity(entity);

            _transferRepositoryMock.Verify(m => m.Update(It.IsAny<DbContext>(), It.IsAny<TransferEntity>()));
        }

        [TestMethod]
        public void GetPackageLogListTest()
        {
            var count = 10;

            var packageLogsToGet = Enumerable
                .Range(1, 10)
                .Select(x => new Common.PackageManager.Storage.Package { PackageId = x.ToString() })
                .AsQueryable();
            _packageRepositoryMock
                .Setup(m => m.GetAll(It.IsAny<DbContext>()))
                .Returns(packageLogsToGet);

            var manager = CreatePackageManager();

            var packageLogs = manager.PackageEntityList;

            Assert.AreEqual(count, packageLogs.Count);

            _packageRepositoryMock.Verify(m => m.GetAll(It.IsAny<DbContext>()));
        }

        [TestMethod]
        public void WhenHasModuleExpectTrue()
        {
            var module = new Module { ModuleId = "moduleId" };
            _moduleRepositoryMock
                .Setup(m => m.GetModuleByModuleId(It.IsAny<DbContext>(), module.ModuleId))
                .Returns(module);

            var manager = CreatePackageManager();

            var result = manager.HasModule(module.ModuleId);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void WhenHasModuleExpectFalse()
        {
            _moduleRepositoryMock
                .Setup(m => m.GetModuleByModuleId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns((Module)null);

            var manager = CreatePackageManager();

            var result = manager.HasModule("1");

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void WhenHasScriptExpectTrue()
        {
            var script = new Script { ScriptId = 1 };
            _scriptRepositoryMock
                .Setup(m => m.GetScriptByScriptId(It.IsAny<DbContext>(), script.ScriptId))
                .Returns(script);

            var manager = CreatePackageManager();

            var result = manager.HasScript(script.ScriptId);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void WhenHasScriptExpectFalse()
        {
            _scriptRepositoryMock
                .Setup(m => m.GetScriptByScriptId(It.IsAny<DbContext>(), It.IsAny<int>()))
                .Returns((Script)null);

            var manager = CreatePackageManager();

            var result = manager.HasScript(1);

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void WhenIsTranferingTransferStatePendingExpectTrue()
        {
            var transfer = new TransferEntity
            {
                PackageId = "1",
                State = TransferState.Pending
            };

            _transferRepositoryMock
                .Setup(m => m.GetByPackageId(It.IsAny<DbContext>(), transfer.PackageId))
                .Returns(transfer);

            var manager = CreatePackageManager(false);

            var result = manager.IsTransferring(transfer.PackageId);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void WhenIsTranferingTransferStateInProgressExpectTrue()
        {
            var transfer = new TransferEntity
            {
                PackageId = "1",
                State = TransferState.InProgress
            };

            _transferRepositoryMock
                .Setup(m => m.GetByPackageId(It.IsAny<DbContext>(), transfer.PackageId))
                .Returns(transfer);

            var manager = CreatePackageManager(false);

            var result = manager.IsTransferring(transfer.PackageId);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void WhenIsTranferingTransferExpectFalse()
        {
            _transferRepositoryMock
                .Setup(m => m.GetByPackageId(It.IsAny<DbContext>(), "1"))
                .Returns((TransferEntity)null);

            var manager = CreatePackageManager(false);

            var result = manager.IsTransferring("1");

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void WhenHasPackageStateDeletedExpectFalse()
        {
            var package = new Common.PackageManager.Storage.Package
            {
                PackageId = "1",
                State = PackageState.Deleted
            };

            _packageRepositoryMock
                .Setup(m => m.GetPackageByPackageId(It.IsAny<DbContext>(), package.PackageId))
                .Returns(package);

            var manager = CreatePackageManager();

            var result = manager.HasPackage(package.PackageId);

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void WhenHasPackageExpectTrue()
        {
            var package = new Common.PackageManager.Storage.Package
            {
                PackageId = "1",
                State = PackageState.Available
            };

            _packageRepositoryMock
                .Setup(m => m.GetPackageByPackageId(It.IsAny<DbContext>(), package.PackageId))
                .Returns(package);

            var manager = CreatePackageManager();

            var result = manager.HasPackage(package.PackageId);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void WhenHasPackageNotExistsButTransferExpectTrue()
        {
            var transfer = new TransferEntity
            {
                PackageId = "1",
                State = TransferState.Pending
            };

            _transferRepositoryMock
                .Setup(m => m.GetByPackageId(It.IsAny<DbContext>(), transfer.PackageId))
                .Returns(transfer);

            _packageRepositoryMock
                .Setup(m => m.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns((Common.PackageManager.Storage.Package)null);

            var manager = CreatePackageManager(false);

            var result = manager.HasPackage("1");

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatePackageWithNullPackageExpectException()
        {
            var manager = CreatePackageManager();

            manager.CreatePackage(null, null, true, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenInstallPackageWithNullPackageExpectException()
        {
            var manager = CreatePackageManager();

            manager.InstallPackage(null, arg => { }, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenUninstallModuleWithNullModuleExpectException()
        {
            var manager = CreatePackageManager();

            manager.UninstallModule(null, arg => { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenUninstallPackageWithNullPackageExpectException()
        {
            var manager = CreatePackageManager();

            manager.UninstallPackage(null, arg => { }, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenUploadPackageWithNullTransferExpectException()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var manager = CreatePackageManager();

            manager.UploadPackage(
                "packageId",
                null,
                arg => { },
                new CancellationToken(),
                packageLog);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeletePackageWithNullPackageExpectException()
        {
            var manager = CreatePackageManager();

            manager.DeletePackage(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRetryTransferWithNullPackageIdExpectException()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var manager = CreatePackageManager();

            manager.RetryTransfer(null, arg => { }, packageLog, new CancellationToken());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDownloadPackageWithNullTransferExpectException()
        {
            var manager = CreatePackageManager();
            var packageLog = new PackageLog() { PackageId = "Test" };
            manager.DownloadPackage(
                "packageId",
                null,
                arg => { },
                new CancellationToken(),
                packageLog,
                1);
        }

        [TestMethod]
        public void WhenToXmlWithNullArgExpectEmptyString()
        {
            var manager = CreatePackageManager();

            var result = manager.ToXml<object>(null);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void WhenToXmlExpectSerializedData()
        {
            var manager = CreatePackageManager();

            var dataToSerialize = "data to serialize";
            var result = manager.ToXml(dataToSerialize);

            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void WhenParseXmlExpectValidResult()
        {
            var manager = CreatePackageManager();

            var dataToSerialize = "data to serialize";
            var serializedData = manager.ToXml(dataToSerialize);

            var deserializationResult = manager.ParseXml<string>(serializedData);

            Assert.AreEqual(dataToSerialize, deserializationResult);
        }

        private PackageManager CreatePackageManager(bool inittransferRepository = true)
        {
            if (inittransferRepository)
            {
                _transferRepositoryMock.Setup(a => a.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                    .Returns(new TransferEntity());
            }

            return new PackageManager(
                _contextFactoryMock.Object,
                _packageRepositoryMock.Object,
                _transferRepositoryMock.Object,
                _transferSerivceMock.Object,
                _pathMapperMock.Object,
                _packageErrorRepositoryMock.Object,
                _scriptRepositoryMock.Object,
                _moduleRepositoryMock.Object,
                _hashCalculator.Object,
                _componentRegisteryMock.Object,
                _packageLogRepositoryMock.Object,
                _installerServiceMock.Object,
                _fileSystemProviderMock.Object);
        }
    }
}

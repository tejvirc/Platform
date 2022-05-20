namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Application.Contracts.Authentication;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common.Storage;
    using Contracts.Client;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    /// <summary>
    ///     Contains the tests for the RomSignatureVerificationHandlerTest class
    /// </summary>
    [TestClass]
    public class RomSignatureVerificationHandlerTest
    {
        private const int TimeoutWait = 1000;  // one second
        private const byte ClientNumber = 21;

        private static readonly Component TestModule = new Component
        {
            Available = true,
            ComponentId = "ATI_Test_Module",
            FileSystemType = FileSystemType.File,
            Path = "C:\\TestingOSPath\\ATI_Test_Module.iso",
            Size = 1024,
            Type = ComponentType.Module
        };

        private static readonly Component TestOs = new Component
        {
            Available = true,
            ComponentId = "ATI_Test_OS",
            FileSystemType = FileSystemType.File,
            Path = "C:\\TestingOSPath",
            Size = 1024,
            Type = ComponentType.OS
        };

        private static readonly Component TestBoot = new Component
        {
            Available = true,
            ComponentId = "ATI_Test_Boot",
            FileSystemType = FileSystemType.File,
            Path = "C:\\TestingBootPath",
            Size = 1024,
            Type = ComponentType.OS
        };

        private static readonly IList<Component> TestComponents = new List<Component>
        {
            TestOs,
            TestBoot,
            TestModule
        };

        private RomSignatureVerificationHandler _target;
        private Mock<IAuthenticationService> _authenticationService;
        private Mock<ISasHost> _sasHost;
        private Mock<IFileSystemProvider> _fileSystemProvider;
        private Mock<IComponentRegistry> _componentRegistry;
        private AutoResetEvent _waiter;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _sasHost = new Mock<ISasHost>(MockBehavior.Default);
            _authenticationService = new Mock<IAuthenticationService>(MockBehavior.Default);
            _componentRegistry = new Mock<IComponentRegistry>(MockBehavior.Default);
            _fileSystemProvider = new Mock<IFileSystemProvider>(MockBehavior.Default);
            _target = new RomSignatureVerificationHandler(
                _sasHost.Object,
                _componentRegistry.Object,
                _fileSystemProvider.Object,
                _authenticationService.Object);
            _waiter = new AutoResetEvent(false);
        }

        [TestCleanup]
        public void MyTestCleanUp()
        {
            _waiter.Dispose();
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.RomSignatureVerification));
        }

        [DataRow(false, true, true, true, DisplayName = "Null SAS Host")]
        [DataRow(true, false, true, true, DisplayName = "Null Authentication Service")]
        [DataRow(true, true, false, true, DisplayName = "Null Component Registry")]
        [DataRow(true, true, true, false, DisplayName = "Null File System Provider")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentTest(
            bool sasHost,
            bool authenticationService,
            bool componentRegistry,
            bool fileSystemProvider)
        {
            _target = new RomSignatureVerificationHandler(
                sasHost ? _sasHost.Object : null,
                componentRegistry ? _componentRegistry.Object : null,
                fileSystemProvider ? _fileSystemProvider.Object : null,
                authenticationService ? _authenticationService.Object : null);
        }

        [DataRow(ushort.MaxValue, (ushort)0x2345, (ushort)0x7652, (ushort)0x7652, true, true, true, DisplayName = "Rom Signature Calculated with OS and Modules")]
        [DataRow(ushort.MaxValue, (ushort)0x2345, (ushort)0x7652, (ushort)0x2345, false, true, true, DisplayName = "Rom Signature Calculated with OS and Modules missing manifest files")]
        [DataRow(ushort.MaxValue, (ushort)0x2345, (ushort)0x7652, (ushort)0x7652, true, false, true, DisplayName = "Rom Signature Calculated with no registered OS and registered Modules")]
        [DataRow(ushort.MaxValue, (ushort)0x2345, (ushort)0x7652, (ushort)0x2345, true, true, false, DisplayName = "Rom Signature Calculated with registered OS and no registered Modules")]
        [DataRow(ushort.MaxValue, (ushort)0x2345, (ushort)0x7652, ushort.MaxValue, true, false, false, DisplayName = "Rom Signature Calculated with no registered OS and Modules")]
        [DataTestMethod]
        public void CrcCalculationFinishingCallsCrcCalculated(
            ushort seed,
            ushort osHash,
            ushort manifestHash,
            ushort expectedResult,
            bool manifestFound,
            bool osRegistered,
            bool moduleRegistered)
        {
            _componentRegistry.Setup(x => x.Components).Returns(
                TestComponents.Where(
                    x => x.Type == ComponentType.OS && osRegistered ||
                         x.Type == ComponentType.Module && moduleRegistered));

            var testManifest = new byte[] { 0x25, 0x31, 0x52, 0x22 };
            var memoryStream = new MemoryStream(testManifest);
            ConfigureFileSetup(memoryStream, TestModule, manifestFound);

            _authenticationService.Setup(
                x => x.CalculateHash(
                    It.Is<Component>(c => c.Type == ComponentType.OS),
                    It.Is<ComponentVerification>(v => v.AlgorithmType == AlgorithmType.Crc16))).Callback(
                (Component component, ComponentVerification verification) =>
                    verification.Result = BitConverter.GetBytes(osHash));

            _authenticationService.Setup(
                x => x.ComputeHash(
                    memoryStream,
                    AlgorithmType.Crc16.ToString().ToUpperInvariant(),
                    null,
                    It.Is<byte[]>(
                        key => osRegistered
                            ? BitConverter.GetBytes(osHash).SequenceEqual(key)
                            : BitConverter.GetBytes(seed).SequenceEqual(key)),
                    0,
                    -1))
                .Returns(BitConverter.GetBytes(manifestHash));

            var data = new RomSignatureData { Seed = seed, ClientNumber = ClientNumber };

            // Once we get a valid CRC completion we let the SAS client know
            _sasHost.Setup(c => c.RomSignatureCalculated(expectedResult, ClientNumber)).Callback(() => _waiter.Set());
            var response = _target.Handle(data);

            Assert.IsTrue(_waiter.WaitOne(TimeoutWait));
            Assert.IsNotNull(response);
        }

        private void ConfigureFileSetup(Stream fs, Component component, bool manifestFound)
        {
            var manifest = $"{component.ComponentId}.manifest";
            _fileSystemProvider
                .Setup(
                    x => x.SearchFiles(
                        Path.GetDirectoryName(component.Path),
                        $"{Path.GetFileNameWithoutExtension(component.Path)}.manifest"))
                .Returns(manifestFound ? new[] { manifest } : new string[] { });
            _fileSystemProvider.Setup(x => x.GetFileReadStream(manifest)).Returns(fs);
        }
    }
}
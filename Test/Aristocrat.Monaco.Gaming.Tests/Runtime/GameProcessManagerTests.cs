namespace Aristocrat.Monaco.Gaming.Tests.Runtime
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using Contracts;
    using Gaming.Runtime;
    using Gaming.Runtime.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameProcessManagerTests
    {
        private const string InvalidRootDir = @"c:\Temp";
        private const string GamesPath = @"Games";
        private const string Variation = @"VAR_99";
        private const string Jurisdiction = @"Test";

        private readonly long _denom = 5;
        private readonly string _gameDllPath = "TestGame.dll";
        private readonly string _logPath = Directory.GetCurrentDirectory();
        private readonly IntPtr _bottomHwnd = new IntPtr(1);
        private readonly IntPtr _topHwnd = new IntPtr(2);
        private readonly IntPtr _topperHwnd = new IntPtr(3);
        private readonly IntPtr _virtualButtonDeckHwnd = new IntPtr(4);

        private Mock<IEventBus> _eventBus;
        private Mock<IPathMapper> _pathMapper;
        private Mock<IClientEndpointProvider<IRuntime>> _endPointProvider;

        private GameProcessManager _target;

        [TestInitialize]
        public void Initialize()
        {
            _pathMapper = new Mock<IPathMapper>();
            _eventBus = new Mock<IEventBus>();
            _endPointProvider = new Mock<IClientEndpointProvider<IRuntime>>();

            _pathMapper.Setup(m => m.GetDirectory(It.Is<string>(s => s == GamingConstants.GamesPath)))
                .Returns(new DirectoryInfo(InvalidRootDir));
            _pathMapper.Setup(m => m.GetDirectory(It.Is<string>(s => s == GamingConstants.RuntimePath)))
                .Returns(new DirectoryInfo(InvalidRootDir));
        }

        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException(bool nullPathMapper = false,
            bool nullEvent = false,
            bool nullClientEndpointProvider = false)
        {
            SetupGameProcessManager(nullPathMapper, nullEvent, nullClientEndpointProvider);

            Assert.IsNull(_target);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            SetupGameProcessManager();

            Assert.IsNotNull(_target);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenStartWithNullPathExpectException()
        {
            SetupGameProcessManager();

            _target.StartProcess(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenStartWithNullArgsExpectException()
        {
            SetupGameProcessManager();

            _target.StartProcess(GamesPath, null);
        }

        [TestMethod]
        [ExpectedException(typeof(Win32Exception))]
        public void WhenStartWithInvalidPathExpectException()
        {
            SetupGameProcessManager();

            // Looks legit, but the path we're using doesn't exist
            _target.StartProcess(
                GamesPath,
                new GameProcessArgs(
                    Variation,
                    _denom,
                    _bottomHwnd,
                    _topHwnd,
                    _virtualButtonDeckHwnd,
                    _topperHwnd,
                    _gameDllPath,
                    _logPath,
                    Jurisdiction,
                    false,
                    -1,
                    string.Empty));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WhenEndWithNoProcessExpectException()
        {
            SetupGameProcessManager();

            _endPointProvider.Setup(x => x.Clear()).Verifiable();

            _target.EndProcess(-1);

            _endPointProvider.Verify(x => x.Clear(), Times.Exactly(1));
        }

        [TestMethod]
        public void WhenGetWithNoProcessExpectEmptyResults()
        {
            SetupGameProcessManager();

            var result = _target.GetRunningProcesses();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count(), 0);
        }

        public void WhenNullExpectException(
            bool nullPathMapper = false,
            bool nullEvent = false,
            bool nullClientEndpointProvider = false)
        {
            SetupGameProcessManager(nullPathMapper,
                nullEvent,
                nullClientEndpointProvider);
        }

        public void SetupGameProcessManager(
            bool nullPathMapper = false,
            bool nullEvent = false,
            bool nullClientEndpointProvider = false)
        {
            _target = new GameProcessManager(nullPathMapper ? null : _pathMapper.Object,
                nullEvent ? null : _eventBus.Object,
                nullClientEndpointProvider ? null : _endPointProvider.Object);
        }
    }
}
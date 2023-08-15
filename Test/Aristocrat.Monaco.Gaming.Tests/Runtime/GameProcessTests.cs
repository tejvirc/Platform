namespace Aristocrat.Monaco.Gaming.Tests.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Aristocrat.Monaco.Hardware.Contracts.Display;
    using Contracts;
    using Contracts.Models;
    using Contracts.Process;
    using Gaming.Runtime;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Summary description for GameProcessTests
    /// </summary>
    [TestClass]
    public class GameProcessTests
    {
        private const string InvalidRootDir = @"c:\Temp";
        private const int GameId = 1;
        private const int Denom = 5000;
        private const string Variation = @"99";
        private const string GameFolder = @"c:\game";
        private const int Failed = -1;
        private const int ProcessId = 99;
        private const int Success = ProcessId;
        private readonly IntPtr _bottomHwnd = new IntPtr(1);
        private readonly List<long> _denoms = new List<long> { 1000, 5000, 2500 };
        private readonly IntPtr _topHwnd = new IntPtr(2);
        private readonly IntPtr _virtualButtonDeckHwnd = new IntPtr(3);

        private Mock<IPathMapper> _pathMapper;
        private Mock<IProcessManager> _process;
        private Mock<IPropertiesManager> _properties;
        private Mock<IDisplayService> _display;

        [TestInitialize]
        public void Initialize()
        {
            _pathMapper = new Mock<IPathMapper>();
            _process = new Mock<IProcessManager>();
            _properties = new Mock<IPropertiesManager>();
            _display = new Mock<IDisplayService>();

            _display.Setup(x => x.MaximumFrameRate).Returns(-1);
            _pathMapper.Setup(m => m.GetDirectory(It.Is<string>(s => s == GamingConstants.GamesPath)))
                .Returns(new DirectoryInfo(InvalidRootDir));
            _pathMapper.Setup(m => m.GetDirectory(It.Is<string>(s => s == GamingConstants.RuntimePath)))
                .Returns(new DirectoryInfo(InvalidRootDir));
        }

        [DataTestMethod]
        [DataRow(true, false, false, false)]
        [DataRow(false, true, false, false)]
        [DataRow(false, false, true, false)]
        [DataRow(false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullContructorArgumentsTest(
            bool nullProperties,
            bool nullProcess,
            bool nullPathMapper,
            bool nullDisplay)
        {
            CreateTarget(nullProperties, nullProcess, nullPathMapper, nullDisplay);
        }

        [TestMethod]
        public void WhenStartUnknownGameExpectNegativeOne()
        {
            var process = CreateTarget();

            var result = process.StartGameProcess(new GameInitRequest { GameId = GameId, Denomination = Denom });

            Assert.AreEqual(result, Failed);
        }

        [TestMethod]
        public void WhenStartGameExpectSuccess()
        {
            var gameDetail = new Mock<IGameDetail>();
            gameDetail.SetupGet(g => g.Id).Returns(GameId);
            gameDetail.SetupGet(g => g.ThemeId).Returns("Test");
            gameDetail.SetupGet(g => g.VariationId).Returns(Variation);
            gameDetail.SetupGet(g => g.PaytableId).Returns(Variation);
            gameDetail.SetupGet(g => g.Folder).Returns(GameFolder);
            gameDetail.SetupGet(g => g.Enabled).Returns(true);
            gameDetail.SetupGet(g => g.ActiveDenominations).Returns(_denoms);
            gameDetail.SetupGet(g => g.TargetRuntime).Returns(string.Empty);

            _properties.Setup(p => p.GetProperty(GamingConstants.Games, null))
                .Returns(new List<IGameDetail> { gameDetail.Object });

            _process.Setup(p => p.StartProcess(It.IsAny<ProcessStartInfo>()))
                .Returns(Success);

            _pathMapper.Setup(m => m.GetDirectory(@"/Logs"))
                .Returns(new DirectoryInfo(Directory.GetCurrentDirectory()));

            var process = CreateTarget();

            var result = process.StartGameProcess(
                new GameInitRequest
                {
                    GameId = GameId,
                    Denomination = Denom,
                    GameBottomHwnd = _bottomHwnd,
                    GameTopHwnd = _topHwnd,
                    GameVirtualButtonDeckHwnd = _virtualButtonDeckHwnd,
                    GameTopperHwnd = IntPtr.Zero
                });

            Assert.AreEqual(result, Success);

            _process.Verify(
                p => p.StartProcess(
                    It.Is<ProcessStartInfo>(
                        args =>
                            args.CreateNoWindow == false &&
                            args.UseShellExecute == false &&
                            args.ErrorDialog == false
                        )));
        }

        [TestMethod]
        public void WhenEndGameExpectSuccess()
        {
            _process.Setup(p => p.GetRunningProcesses())
                .Returns(new List<int> { ProcessId });

            var process = CreateTarget();

            process.EndGameProcess();

            _process.Verify(p => p.EndProcess(It.Is<int>(id => id == ProcessId), true, true));
        }

        private GameProcess CreateTarget(
            bool nullProperties = false,
            bool nullProcess = false,
            bool nullPathMapper = false,
            bool nullDisplay = false)
        {
            return new GameProcess(
                nullProperties ? null : _properties.Object,
                nullProcess ? null : _process.Object,
                nullPathMapper ? null : _pathMapper.Object,
                nullDisplay ? null : _display.Object);
        }
    }
}
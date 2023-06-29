namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.IO.MemoryMappedFiles;
    using System.Linq;
    using System.Threading;
    using Contracts.EdgeLighting;
    using Hardware.EdgeLight.Contracts;
    using Hardware.EdgeLight.Manager;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class EdgeLightSharedMemoryManagerTest
    {
        private readonly List<int> _gameStripIdsList = new();
        private int _brightnessSentFromGame;
        private Header _header;
        private Mock<ILogicalStripFactory> _logicalStripFactoryMoq = new();
        private List<Mock<IStrip>> _logicalStripMocks;
        private List<IStrip> _logicalStrips;
        private MemoryMappedFile _sharedMem;
        private Mutex _sharedMemMutex;
        private MemoryMappedViewStream _sharedMemoryStream;
        private SharedMemoryManager _sharedMemoryManager;
        private List<Color> _colors;

        private string SharedMutexName { get; set; } = "TestEdgeLightSharedMutex";

        private string SharedMemoryName { get; set; } = "TestEdgeLightSharedMemory";

        [TestInitialize]
        public void Setup()
        {
            _logicalStripMocks = new List<Mock<IStrip>>
            {
                new(),
                new(),
                new(),
                new(),
                new(MockBehavior.Strict)
            };

            _logicalStripFactoryMoq = new Mock<ILogicalStripFactory>(MockBehavior.Strict);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _header = new Header();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _sharedMemMutex?.Dispose();
            _sharedMem?.Dispose();
            _sharedMemoryManager?.Dispose();
        }

        [TestMethod]
        public void CallingWithDefault()
        {
            _sharedMemoryManager = new SharedMemoryManager(new SharedMemoryInformation());
        }

        [TestMethod]
        public void CallingDisposeAfterAlreadyDisposed()
        {
            _sharedMemoryManager = new SharedMemoryManager(new SharedMemoryInformation());
            _sharedMemoryManager.Dispose();
            _sharedMemoryManager.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenMutexNameIsNull()
        {
            SetupSharedMemory(false);
            OpenExistingMemory(SharedMemoryName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSharedMemoryNameIsNull()
        {
            SetupSharedMemory(false);
            OpenExistingMemory(null, SharedMutexName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenMutexNameIsEmpty()
        {
            SetupSharedMemory(false);
            OpenExistingMemory(SharedMemoryName, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenSharedMemoryNameIsEmpty()
        {
            SetupSharedMemory(false);
            OpenExistingMemory("", SharedMutexName);
        }

        [TestMethod]
        [ExpectedException(typeof(WaitHandleCannotBeOpenedException))]
        public void WhenMutexNameDoesNotExist()
        {
            SetupSharedMemory(false);
            OpenExistingMemory(SharedMemoryName, "someMutex");
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void WhenSharedMemoryNameDoesNotExist()
        {
            SetupSharedMemory(false);
            OpenExistingMemory("someMemoryName", SharedMutexName);
        }

        [TestMethod]
        public void WriteAndReadSameGameDataToAndFromSharedMemory()
        {
            SetupSharedMemory(false);
            OpenExistingMemory(SharedMemoryName, SharedMutexName);
            _brightnessSentFromGame = new Random().Next(101);
            _gameStripIdsList.Add(1);
            _gameStripIdsList.Add(2);
            _gameStripIdsList.Add(3);
            _gameStripIdsList.Add(4);
            _gameStripIdsList.Add(5);
            _gameStripIdsList.Add(6);
            _gameStripIdsList.Add(7);
            _gameStripIdsList.Add(8);
            DoAccessorAction(
                x =>
                {
                    var brightness = _brightnessSentFromGame;
                    var gameStripIDs = _gameStripIdsList;
                    WriteGameData(x, brightness, gameStripIDs);
                });

            var gameData = _sharedMemoryManager.GameData;
            Assert.AreEqual(_brightnessSentFromGame, gameData.Brightness);
            CollectionAssert.AreEquivalent(_gameStripIdsList, gameData.ControlledStrips.ToList());
        }

        [TestMethod]
        public void WriteAndReadDifferentGameDataToAndFromSharedMemory_1()
        {
            SetupSharedMemory(false);
            OpenExistingMemory(SharedMemoryName, SharedMutexName);
            _brightnessSentFromGame = new Random().Next(101);
            _gameStripIdsList.Add(1);
            _gameStripIdsList.Add(2);
            _gameStripIdsList.Add(3);
            _gameStripIdsList.Add(4);
            _gameStripIdsList.Add(5);
            _gameStripIdsList.Add(6);
            _gameStripIdsList.Add(7);
            _gameStripIdsList.Add(8);
            DoAccessorAction(
                x =>
                {
                    var brightness = _brightnessSentFromGame;
                    var gameStripIDs = _gameStripIdsList;
                    WriteGameData(x, brightness, gameStripIDs);
                });

            var gameData = _sharedMemoryManager.GameData;
            Assert.AreEqual(_brightnessSentFromGame, gameData.Brightness);
            CollectionAssert.AreEquivalent(_gameStripIdsList, gameData.ControlledStrips.ToList());
        }

        [TestMethod]
        public void WriteAndReadDifferentGameDataToAndFromSharedMemory_2()
        {
            SetupSharedMemory(false);
            OpenExistingMemory(SharedMemoryName, SharedMutexName);
            _brightnessSentFromGame = new Random().Next(101);
            _gameStripIdsList.Add(1);
            _gameStripIdsList.Add(2);
            _gameStripIdsList.Add(3);
            _gameStripIdsList.Add(4);
            _gameStripIdsList.Add(5);
            _gameStripIdsList.Add(6);
            _gameStripIdsList.Add(7);
            _gameStripIdsList.Add(8);
            DoAccessorAction(
                x =>
                {
                    var brightness = _brightnessSentFromGame;
                    var gameStripIDs = _gameStripIdsList;
                    WriteGameData(x, brightness, gameStripIDs);
                });

            _gameStripIdsList.Remove(8);
            var gameData = _sharedMemoryManager.GameData;
            Assert.AreEqual(_brightnessSentFromGame, gameData.Brightness);
            CollectionAssert.AreNotEquivalent(_gameStripIdsList, gameData.ControlledStrips.ToList());
        }

        [TestMethod]
        public void ReadAndValidateHeaderDataFromSharedMemory_1()
        {
            SetupSharedMemory(true);
            OpenExistingMemory(SharedMemoryName, SharedMutexName);
            var readPlatformData = new List<int>();
            DoAccessorAction(x => { readPlatformData = new List<int>(ReadHeaderData(x)); });
            CollectionAssert.AreEquivalent(readPlatformData, _header.DataBytes);
        }

        [TestMethod]
        public void ReadAndValidateHeaderDataFromSharedMemory_2()
        {
            SetupSharedMemory(true);
            OpenExistingMemory(SharedMemoryName, SharedMutexName);
            var readPlatformData = new List<int>();
            DoAccessorAction(x => { readPlatformData = new List<int>(ReadHeaderData(x)); });
            Assert.IsTrue(readPlatformData.SequenceEqual(_header.DataBytes));
            Assert.IsTrue(
                readPlatformData.SequenceEqual(
                    new[]
                    {
                        _header.TotalSize, _header.Version, _header.HeaderSize, _header.PlatformOffset,
                        _header.GameOffset, _header.MaxLedInStrip, _header.MaxStripIdSupported, _header.BytesPerLed,
                        _header.DefaultStripOffset
                    }));
            Assert.IsTrue(
                readPlatformData.Count == 9);
            Assert.IsFalse(
                readPlatformData[0] == 13224);
            Assert.IsFalse(
                readPlatformData[1] == 2);
            Assert.IsFalse(
                readPlatformData[2] == 13224);
            Assert.IsFalse(
                readPlatformData[3] == 9765);
            Assert.IsFalse(
                readPlatformData[4] == 8764);
            Assert.IsFalse(
                readPlatformData[5] == 4513732);
            Assert.IsFalse(
                readPlatformData[6] == 1326532624);
            Assert.IsFalse(
                readPlatformData[7] == 28641);
            Assert.IsFalse(
                readPlatformData[8] == 67874);
        }

        [TestMethod]
        public void WriteAndReadSamePlatformDataToAndFromSharedMemory()
        {
            SetupSharedMemory(true);
            OpenExistingMemory(SharedMemoryName, SharedMutexName);
            var writtenPlatformData = _logicalStrips
                .Select(x => new StripData { StripId = x.StripId, LedCount = x.LedCount })
                .ToList();
            var flatPlatformList = writtenPlatformData.SelectMany(x => new[] { x.StripId, x.LedCount });
            _sharedMemoryManager.SetPlatformStripCount(writtenPlatformData);
            var readPlatformData = (10, new List<int>());
            DoAccessorAction(x => { readPlatformData = ReadPlatformData(x); });
            Assert.AreEqual(writtenPlatformData.Count, readPlatformData.Item1);
            CollectionAssert.AreEquivalent(flatPlatformList.ToList(), readPlatformData.Item2);
        }

        [TestMethod]
        public void WriteAndReadDifferentPlatformDataToAndFromSharedMemory_1()
        {
            SetupSharedMemory(true);
            OpenExistingMemory(SharedMemoryName, SharedMutexName);
            var writtenPlatformData = _logicalStrips
                .Select(x => new StripData { StripId = x.StripId, LedCount = x.LedCount })
                .ToList();
            var flatPlatformList = writtenPlatformData.SelectMany(x => new[] { x.StripId, x.LedCount });
            _sharedMemoryManager.SetPlatformStripCount(writtenPlatformData);
            var readPlatformData = (10, new List<int>());
            DoAccessorAction(x => { readPlatformData = ReadPlatformData(x); });
            Assert.AreNotEqual(writtenPlatformData.Count + 1, readPlatformData.Item1);
            CollectionAssert.AreEquivalent(flatPlatformList.ToList(), readPlatformData.Item2);
        }

        [TestMethod]
        public void WriteAndReadDifferentPlatformDataToAndFromSharedMemory_2()
        {
            SetupSharedMemory(true);
            OpenExistingMemory(SharedMemoryName, SharedMutexName);
            var writtenPlatformData = _logicalStrips
                .Select(x => new StripData { StripId = x.StripId, LedCount = x.LedCount })
                .ToList();
            var flatPlatformList = writtenPlatformData.SelectMany(x => new[] { x.StripId, x.LedCount }).ToList();
            _sharedMemoryManager.SetPlatformStripCount(writtenPlatformData);
            var readPlatformData = (10, new List<int>());
            DoAccessorAction(x => { readPlatformData = ReadPlatformData(x); });
            Assert.AreEqual(writtenPlatformData.Count, readPlatformData.Item1);
            flatPlatformList.RemoveAt(5);
            CollectionAssert.AreNotEquivalent(flatPlatformList.ToList(), readPlatformData.Item2);
        }

        [TestMethod]
        public void WhenAbandonedMutexExceptionIsRaised()
        {
            SetupSharedMemory(true);
            OpenExistingMemory(SharedMemoryName, SharedMutexName);
            var t = new Thread(AbandonMutex);
            t.Start();
            t.Join();
            var writtenPlatformData = _logicalStrips
                .Select(x => new StripData { StripId = x.StripId, LedCount = x.LedCount })
                .ToList();
            var flatPlatformList = writtenPlatformData.SelectMany(x => new[] { x.StripId, x.LedCount }).ToList();
            // When we have abandon mutex the mutex will be just created, without retrying to perform
            // the read/write action. Thus you need to retry the action.
            _sharedMemoryManager.SetPlatformStripCount(writtenPlatformData);
            var readPlatformData = (10, new List<int>());
            DoAccessorAction(x => { readPlatformData = ReadPlatformData(x); });
            Assert.IsTrue(readPlatformData.Item1 == 0);
            Assert.IsTrue(readPlatformData.Item2.Count == 0);
            _sharedMemoryManager.SetPlatformStripCount(writtenPlatformData);
            DoAccessorAction(x => { readPlatformData = ReadPlatformData(x); });
            Assert.AreEqual(writtenPlatformData.Count, readPlatformData.Item1);
            flatPlatformList.RemoveAt(5);
            Assert.IsFalse(flatPlatformList.SequenceEqual(readPlatformData.Item2));
        }

        private void SetupLogicalStrips()
        {
            _logicalStrips = _logicalStripMocks.Select(x => x.Object).ToList();
            _logicalStripFactoryMoq.Setup(x => x.GetLogicalStrips(It.IsAny<IReadOnlyCollection<IStrip>>()))
                .Returns(_logicalStrips);
            var stripId = 0;
            foreach (var logicalStripMock in _logicalStripMocks)
            {
                logicalStripMock.SetupGet(x => x.StripId).Returns(stripId++);
                logicalStripMock.SetupGet(x => x.LedCount).Returns(24);
            }
        }

        private void SetupSharedMemory(
            bool setupLogicalStrips,
            string sharedMemoryName = null,
            string sharedMutexName = null)
        {
            if (setupLogicalStrips)
            {
                SetupLogicalStrips();
            }

            SharedMemoryName = sharedMemoryName ?? SharedMemoryName;
            SharedMutexName = sharedMutexName ?? SharedMutexName;
            _sharedMemoryManager?.Dispose();
            _sharedMemoryManager = new SharedMemoryManager(
                new SharedMemoryInformation { MemoryName = SharedMemoryName, MutexName = SharedMutexName });
        }

        private void OpenExistingMemory(string sharedMemoryName, string sharedMutexName)
        {
            _sharedMemMutex?.Dispose();
            _sharedMem?.Dispose();
            _sharedMemoryStream?.Dispose();
            _sharedMemMutex = Mutex.OpenExisting(sharedMutexName);
            _sharedMem = MemoryMappedFile.OpenExisting(sharedMemoryName, MemoryMappedFileRights.ReadWrite);
            _sharedMemoryStream = _sharedMem.CreateViewStream();
        }

        private IList<int> ReadHeaderData(UnmanagedMemoryAccessor accessor)
        {
            const int start = 0;
            var strips = new int[_header.DataBytes.Length];
            accessor.ReadArray(start, strips, 0, strips.Length);
            return strips.ToList();
        }

        private (int stripCount, List<int> platfromData) ReadPlatformData(UnmanagedMemoryAccessor accessor)
        {
            var start = _header.PlatformOffset;
            var stripCount = accessor.ReadInt32(start);
            start += sizeof(int);
            var strips = new int[stripCount * 2];
            accessor.ReadArray(start, strips, 0, strips.Length);
            return (stripCount, strips.ToList());
        }

        private void WriteGameData(UnmanagedMemoryAccessor accessor, int brightness, IReadOnlyCollection<int> strips)
        {
            var start = _header.GameOffset;
            accessor.Write(start, brightness);
            start += sizeof(int);
            accessor.Write(start, strips.Count);
            start += sizeof(int);
            accessor.WriteArray(
                start,
                strips.ToArray(),
                0,
                strips.ToArray().Length);
        }

        private void DoAccessorAction(Action<MemoryMappedViewAccessor> action)
        {
            try
            {
                _sharedMemMutex.WaitOne();
                using (var accessor = _sharedMem.CreateViewAccessor(
                           0,
                           _header.TotalSize,
                           MemoryMappedFileAccess.ReadWrite))
                {
                    action(accessor);
                }
            }
            finally
            {
                // Let runtime host access the shared memory for writing.
                _sharedMemMutex.ReleaseMutex();
            }
        }

        private void SetData(int stripId, List<Color> colorList = null)
        {
            _colors = colorList ?? new List<Color> { Color.Blue, Color.Black, Color.Transparent, Color.White };
            WritePlatformData(_colors.ToArray(), stripId);
        }

        private void WritePlatformData(IEnumerable<Color> colors, int stripId)
        {
            try
            {
                _sharedMemMutex.WaitOne();
                _sharedMemoryStream.Seek(
                    _header.HeaderSize +
                    (stripId & 0xFF)
                    * EdgeLightConstants.StrideLength,
                    SeekOrigin.Begin);
                foreach (var color in colors)
                {
                    var temp = BitConverter.GetBytes(color.ToArgb()).Reverse().ToArray();
                    var count = temp.Length;
                    _sharedMemoryStream.Write(
                        temp,
                        0,
                        count);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                _sharedMemMutex.ReleaseMutex();
            }
        }

        private void AbandonMutex()
        {
            _sharedMemMutex.WaitOne();
            // Abandon the mutexes by exiting without releasing them.
        }
    }
}
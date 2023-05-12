namespace Aristocrat.Monaco.Hardware.Tests.Persistence
{
    using System;
    using System.Data.SqlClient;
    using Microsoft.Data.Sqlite;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Services;
    using Contracts.Persistence;
    using Hardware.StorageAdapters;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Some fake data of all different types, so we can test storing them.
    /// </summary>
    public class TestTransaction
    {
        public long TransactionId { get; set; }
        public long LogSequence { get; set; }
        public DateTime Timestamp { get; set; }
        public int GameId { get; set; }
        public long DenomId { get; set; }
        public long StartCredits { get; set; }
        public long EndCredits { get; set; }
        public long InitialWager { get; set; }
        public long FinalWager { get; set; }
        public long UncommittedWin { get; set; }
        public long InitialWin { get; set; }
        public long SecondaryPlayed { get; set; }
        public long SecondaryWager { get; set; }
        public long SecondaryWin { get; set; }
        public long FinalWin { get; set; }
        public long AmountIn { get; set; }
        public long PostAmountIn { get; set; }
        public long AmountOut { get; set; }
        public string Log { get; set; }
        public short ShortAmount { get; set; }
        public ushort UshortAmount { get; set; }
        public uint UintAmount { get; set; }
        public ulong UlongAmount { get; set; }
        public float FloatAmount { get; set; }
        public double DoubleAmount { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if(!(obj is TestTransaction other))
            {
                return false;
            }

            return TransactionId == other.TransactionId && LogSequence == other.LogSequence &&
                   GameId == other.GameId && DenomId == other.DenomId &&
                   StartCredits == other.StartCredits && EndCredits == other.EndCredits &&
                   InitialWager == other.InitialWager && FinalWager == other.FinalWager &&
                   UncommittedWin == other.UncommittedWin && InitialWin == other.InitialWin &&
                   SecondaryPlayed == other.SecondaryPlayed && SecondaryWager == other.SecondaryWager &&
                   SecondaryWin == other.SecondaryWin && FinalWin == other.FinalWin && AmountIn == other.AmountIn &&
                   PostAmountIn == other.PostAmountIn && AmountOut == other.AmountOut && string.Equals(Log, other.Log) &&
                   ShortAmount == other.ShortAmount && UshortAmount == other.UshortAmount && UintAmount == other.UintAmount &&
                   UlongAmount == other.UlongAmount && FloatAmount == other.FloatAmount && DoubleAmount == other.DoubleAmount;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode();
        }

        /// <summary>
        /// Gets random object.
        /// </summary>
        /// <param name="random">The random.</param>
        /// <returns>The random object.</returns>
        public static TestTransaction GetRandomObject(Random random)
        {
            return new TestTransaction
            {
                TransactionId = random.Next(1, 10000),
                LogSequence = random.Next(1, 10000),
                Timestamp = DateTime.Now,
                GameId = random.Next(1, 100),
                DenomId = random.Next(1, 100),
                StartCredits = random.Next(1, 200000),
                EndCredits = random.Next(1, 200000),
                InitialWager = random.Next(1, 10000),
                FinalWager = random.Next(1, 10000),
                UncommittedWin = random.Next(1, 10000),
                InitialWin = random.Next(1, 10000),
                SecondaryPlayed = random.Next(1, 10000),
                SecondaryWager = random.Next(1, 10000),
                SecondaryWin = random.Next(1, 10000),
                FinalWin = random.Next(1, 10000),
                AmountIn = random.Next(1, 10000),
                PostAmountIn = random.Next(1, 10000),
                AmountOut = random.Next(int.MinValue, int.MaxValue),
                Log = RandomString(2048, random),
                ShortAmount = (short)random.Next(short.MinValue, short.MaxValue),
                UshortAmount = (ushort)random.Next(short.MaxValue, ushort.MaxValue),
                UintAmount = (uint)random.Next(int.MaxValue) + int.MaxValue,
                UlongAmount = (ulong)random.Next(int.MaxValue) * long.MaxValue,
                FloatAmount = (float)(random.NextDouble() - 0.5) * float.MaxValue,
                DoubleAmount = (double)(random.NextDouble() - 0.5) * double.MaxValue
            };
        }

        private int HashCode()
        {
            var hashCode = TransactionId.GetHashCode();
            hashCode = (hashCode * 397) ^ LogSequence.GetHashCode();
            hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
            hashCode = (hashCode * 397) ^ GameId;
            hashCode = (hashCode * 397) ^ DenomId.GetHashCode();
            hashCode = (hashCode * 397) ^ StartCredits.GetHashCode();
            hashCode = (hashCode * 397) ^ EndCredits.GetHashCode();
            hashCode = (hashCode * 397) ^ InitialWager.GetHashCode();
            hashCode = (hashCode * 397) ^ FinalWager.GetHashCode();
            hashCode = (hashCode * 397) ^ UncommittedWin.GetHashCode();
            hashCode = (hashCode * 397) ^ InitialWin.GetHashCode();
            hashCode = (hashCode * 397) ^ SecondaryPlayed.GetHashCode();
            hashCode = (hashCode * 397) ^ SecondaryWager.GetHashCode();
            hashCode = (hashCode * 397) ^ SecondaryWin.GetHashCode();
            hashCode = (hashCode * 397) ^ FinalWin.GetHashCode();
            hashCode = (hashCode * 397) ^ AmountIn.GetHashCode();
            hashCode = (hashCode * 397) ^ PostAmountIn.GetHashCode();
            hashCode = (hashCode * 397) ^ AmountOut.GetHashCode();
            hashCode = (hashCode * 397) ^ (Log?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ ShortAmount.GetHashCode();
            hashCode = (hashCode * 397) ^ UshortAmount.GetHashCode();
            hashCode = (hashCode * 397) ^ UintAmount.GetHashCode();
            hashCode = (hashCode * 397) ^ UlongAmount.GetHashCode();
            hashCode = (hashCode * 397) ^ FloatAmount.GetHashCode();
            hashCode = (hashCode * 397) ^ DoubleAmount.GetHashCode();
            return hashCode;
        }

        private static string RandomString(int length, Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    [TestClass]
    public class PersistenceFacadeTests
    {
        private string _assemblyDirectory;
        private const string DatabaseDirectoryName = @"Data";
        private const string DatabaseFilename = @"PersistenceFacadeTests.sqlite";
        private const string DatabasePassword = @"tk7tjBLQ8GpySFNZTHYD";
        private string _databaseFullPath;

        private Mock<IPathMapper> _pathMapper;
        private Mock<IEventBus> _eventBus;

        private SqlPersistentStorageManager _persistentStorageManager;
        private PersistenceProviderFacade _persistenceProviderFacade;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _pathMapper = MoqServiceManager.CreateAndAddService<IPathMapper>(MockBehavior.Strict);

            MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);

            _assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(_assemblyDirectory))
                throw new NullReferenceException(nameof(_assemblyDirectory));

            var databaseDirectoryPath = Path.Combine(_assemblyDirectory, DatabaseDirectoryName);
            if (!Directory.Exists(databaseDirectoryPath))
            {
                Directory.CreateDirectory(databaseDirectoryPath);
            }

            _databaseFullPath = Path.Combine(databaseDirectoryPath, DatabaseFilename);
            _pathMapper.Setup(m => m.GetDirectory(It.IsAny<string>())).Returns(
                new DirectoryInfo(databaseDirectoryPath));

            _persistentStorageManager = new SqlPersistentStorageManager(_pathMapper.Object, _eventBus.Object,
                DatabaseFilename, DatabasePassword);

            MoqServiceManager.AddService<IPersistentStorageManager>(_persistentStorageManager);

            _persistenceProviderFacade = new PersistenceProviderFacade();
            _persistenceProviderFacade.Initialize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_databaseFullPath))
            {
                using (var connection = CreateConnection())
                {
                    connection.Close();
                }
                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                File.Delete(_databaseFullPath);
            }

            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void TestPersistenceProviderFacade_GetBlock1()
        {
            const string key = @"TestKey";

            var block = _persistenceProviderFacade.GetBlock(key);
            Assert.IsNull(block);
        }

       [TestMethod]
        public void TestPersistenceProviderFacade_GetOrCreateBlock()
        {
            const string key = @"TestKey";

            var block = _persistenceProviderFacade.GetOrCreateBlock(key, PersistenceLevel.Critical);
            Assert.IsNotNull(block);

            block = _persistenceProviderFacade.GetBlock(key);
            Assert.IsNotNull(block);
        }

        [TestMethod]
        public void TestPersistentBlockFacade_GetValue()
        {
            const string key = @"TestKey";
            const string blockName = @"TestBlock";

            var block = _persistenceProviderFacade.GetOrCreateBlock(blockName, PersistenceLevel.Critical);

            Assert.AreEqual(false, block.GetValue<int>(key, out _));
            Assert.AreEqual(false, block.GetValue<string>(key, out _));
            Assert.AreEqual(false, block.GetValue<TestTransaction>(key, out _));
        }

        [TestMethod]
        public void TestPersistentBlockFacade_GetSetValue()
        {
            const string key = @"TestKey";
            const string blockName = @"TestBlock";
            const string data = @"FacadeIsDesignPattern";

            var block = _persistenceProviderFacade.GetOrCreateBlock(blockName, PersistenceLevel.Critical);

            Assert.AreEqual(false, block.GetValue<string>(key, out _));

            Assert.AreEqual(true, block.SetValue(key, data));

            Assert.AreEqual(true, block.GetValue<string>(key, out var result));
            Assert.AreEqual(data, result);
        }

        [TestMethod]
        public void TestPersistentBlockFacade_GetSetValues()
        {
            const string blockName = @"TestBlock";

            const string key1 = @"TestKey1";
            const string key2 = @"TestKey2";
            const string key3 = @"TestKey3";

            var random = new Random();
            var data1 = TestTransaction.GetRandomObject(random);
            var data2 = TestTransaction.GetRandomObject(random);
            var data3 = TestTransaction.GetRandomObject(random);


            var block = _persistenceProviderFacade.GetOrCreateBlock(blockName, PersistenceLevel.Critical);

            Assert.AreEqual(false, block.GetValue<TestTransaction>(key1, out var result1));
            Assert.AreEqual(false, block.GetValue<TestTransaction>(key2, out var result2));
            Assert.AreEqual(false, block.GetValue<TestTransaction>(key3, out var result3));

            Assert.AreEqual(true, block.SetValue(key1, data1));
            Assert.AreEqual(true, block.SetValue(key2, data2));
            Assert.AreEqual(true, block.SetValue(key3, data3));

            Assert.AreEqual(true, block.GetValue(key1, out result1));
            Assert.AreEqual(data1, result1);

            Assert.AreEqual(true, block.GetValue(key2, out result2));
            Assert.AreEqual(data2, result2);

            Assert.AreEqual(true, block.GetValue(key3, out result3));
            Assert.AreEqual(data3, result3);
        }

        [TestMethod]
        public void TestPersistentBlockFacade_GetSetValueWithinScope()
        {
            const string blockName = @"TestBlock";
            const string key = @"TestKey";

            var random = new Random();
            var data = TestTransaction.GetRandomObject(random);
            
            var block = _persistenceProviderFacade.GetOrCreateBlock(blockName, PersistenceLevel.Critical);

            Assert.AreEqual(false, block.GetValue<TestTransaction>(key, out var result));
            Assert.AreEqual(null, result);

            using (var scope = _persistenceProviderFacade.ScopedTransaction())
            {
                Assert.AreEqual(true, block.SetValue(key, data));

                Assert.AreEqual(false, block.GetValue(key, out result));
                Assert.AreEqual(null, result);

                scope.Complete();
            }

            Assert.AreEqual(true, block.GetValue(key, out result));
            Assert.AreEqual(data, result);
        }


        [TestMethod]
        public void TestPersistentBlockFacade_GetSetValueWithinScope2()
        {
            const string blockName = @"TestBlock";
            const string key = @"TestKey";

            var data1 = @"WeMakeMoneyForAristocrat";
            var data2 = @"RealEngineeringAtAristocrat";
            
            var block = _persistenceProviderFacade.GetOrCreateBlock(blockName, PersistenceLevel.Critical);

            Assert.AreEqual(true, block.SetValue(key, data1));

            Assert.AreEqual(true, block.GetValue<string>(key, out var result));
            Assert.AreEqual(data1, result);

            using (var scope = _persistenceProviderFacade.ScopedTransaction())
            {
                Assert.AreEqual(true, block.SetValue(key, data2));

                Assert.AreEqual(true, block.GetValue(key, out result));
                Assert.AreEqual(data1, result);

                scope.Complete();
            }

            Assert.AreEqual(true, block.GetValue(key, out result));
            Assert.AreEqual(data2, result);
        }

        [TestMethod]
        public void TestPersistentBlockFacade_GetSetValueWithinScope3()
        {
            const string block1Name = @"TestBlock1";
            const string block1Key = @"TestKey1";
            var block1Data1 = @"WeMakeMoneyForAristocrat";
            var block1Data2 = @"RealEngineeringAtAristocrat";

            const string block2Name = @"TestBlock2";
            const string block2Key = @"TestKey2";
            var block2Data1 = @"WeMakeMoneyForAristocratAtGreatLengths";
            var block2Data2 = @"RealEngineeringAtAristocratIsWhatWeDo";
            
            var block1 = _persistenceProviderFacade.GetOrCreateBlock(block1Name, PersistenceLevel.Critical);
            var block2 = _persistenceProviderFacade.GetOrCreateBlock(block2Name, PersistenceLevel.Critical);

            Assert.AreEqual(true, block1.SetValue(block1Key, block1Data1));
            Assert.AreEqual(true, block2.SetValue(block2Key, block2Data1));

            Assert.AreEqual(true, block1.GetValue<string>(block1Key, out var block1Result));
            Assert.AreEqual(block1Data1, block1Result);

            Assert.AreEqual(true, block2.GetValue<string>(block2Key, out var block2Result));
            Assert.AreEqual(block2Data1, block2Result);

            using (var scope = _persistenceProviderFacade.ScopedTransaction())
            {
                Assert.AreEqual(true, block1.SetValue(block1Key, block1Data2));
                Assert.AreEqual(true, block1.GetValue(block1Key, out block1Result));
                Assert.AreEqual(block1Data1, block1Result);

                Assert.AreEqual(true,block2.SetValue(block2Key, block2Data2));
                Assert.AreEqual(true, block2.GetValue(block2Key, out block2Result));
                Assert.AreEqual(block2Data1, block2Result);

                scope.Complete();
            }

            Assert.AreEqual(true, block1.GetValue(block1Key, out block1Result));
            Assert.AreEqual(block1Data2, block1Result);

            Assert.AreEqual(true, block2.GetValue(block2Key, out block2Result));
            Assert.AreEqual(block2Data2, block2Result);
        }

        [TestMethod]
        public void TestPersistentBlockTransactionFacade_GetSetValue()
        {
            const string key = @"TestKey";
            const string blockName = @"TestBlock";
            var data = TestTransaction.GetRandomObject(new Random());

            var block = _persistenceProviderFacade.GetOrCreateBlock(blockName, PersistenceLevel.Critical);

            using (var transaction = block.Transaction())
            {
                Assert.AreEqual(false, transaction.GetValue<TestTransaction>(key, out var result));
                Assert.AreEqual(default, result);

                Assert.AreEqual(true, transaction.SetValue(key, data));

                Assert.AreEqual(false, block.GetValue(key, out result));
                Assert.AreEqual(default, result);

                transaction.Commit();

                Assert.AreEqual(true, block.GetValue(key, out result));
                Assert.AreEqual(data, result);
            }
        }

        [TestMethod]
        public void TestPersistentBlockTransactionFacade_GetSetValues()
        {
            const string key1 = @"TestKey1";
            const string key2 = @"TestKey2";
            const string key3 = @"TestKey3";

            const string blockName = @"TestBlock";

            var random = new Random();
            var data1 = TestTransaction.GetRandomObject(random);
            var data2 = TestTransaction.GetRandomObject(random);
            var data3 = TestTransaction.GetRandomObject(random);

            var block = _persistenceProviderFacade.GetOrCreateBlock(blockName, PersistenceLevel.Critical);

            using (var transaction = block.Transaction())
            {
                Assert.AreEqual(false, transaction.GetValue<TestTransaction>(key1, out var result1));
                Assert.AreEqual(false, transaction.GetValue<TestTransaction>(key2, out var result2));
                Assert.AreEqual(false, transaction.GetValue<TestTransaction>(key3, out var result3));

                Assert.AreEqual(default, result1);
                Assert.AreEqual(default, result2);
                Assert.AreEqual(default, result3);

                Assert.AreEqual(true, transaction.SetValue(key1, data1));
                Assert.AreEqual(true, transaction.SetValue(key2, data2));
                Assert.AreEqual(true, transaction.SetValue(key3, data3));

                Assert.AreEqual(false, block.GetValue(key1, out result1));
                Assert.AreEqual(false, block.GetValue(key2, out result2));
                Assert.AreEqual(false, block.GetValue(key3, out result3));

                Assert.AreEqual(default, result1);
                Assert.AreEqual(default, result2);
                Assert.AreEqual(default, result3);

                transaction.Commit();

                Assert.AreEqual(true, block.GetValue(key1, out result1));
                Assert.AreEqual(true, block.GetValue(key2, out result2));
                Assert.AreEqual(true, block.GetValue(key3, out result3));

                Assert.AreEqual(data1, result1);
                Assert.AreEqual(data2, result2);
                Assert.AreEqual(data3, result3);
            }
        }

        [TestMethod]
        public void TestPersistentBlockTransactionFacade_GetSetValueWithinScope()
        {
            const string key = @"TestKey";
            const string blockName = @"TestBlock";

            var random = new Random();
            var data = TestTransaction.GetRandomObject(random);

            var block = _persistenceProviderFacade.GetOrCreateBlock(blockName, PersistenceLevel.Critical);

            using (var scope = _persistentStorageManager.ScopedTransaction())
            {
                using (var transaction = block.Transaction())
                {
                    Assert.AreEqual(true, transaction.SetValue(key, data));
                    transaction.Commit();
                }

                Assert.AreEqual(false, block.GetValue(key, out TestTransaction result));
                Assert.AreEqual(default, result);

                scope.Complete();

                Assert.AreEqual(true, block.GetValue(key, out result));
                Assert.AreEqual(data, result);
            }
        }

        [TestMethod]
        public void TestPersistentBlockTransactionFacade_GetSetValuesWithinScope()
        {
            const string key1 = @"TestKey1";
            const string key2 = @"TestKey2";

            const string blockName = @"TestBlock";

            var random = new Random();
            var data1 = TestTransaction.GetRandomObject(random);
            var data2 = TestTransaction.GetRandomObject(random);

            var block = _persistenceProviderFacade.GetOrCreateBlock(blockName, PersistenceLevel.Critical);

            using (var scope = _persistentStorageManager.ScopedTransaction())
            {
                using (var transaction = block.Transaction())
                {
                    Assert.AreEqual(true, transaction.SetValue(key1, data1));
                    transaction.Commit();
                }

                Assert.AreEqual(false, block.GetValue(key1, out TestTransaction result1));
                Assert.AreEqual(default, result1);

                using (var transaction = block.Transaction())
                {
                    Assert.AreEqual(true, transaction.SetValue(key2, data2));
                    transaction.Commit();
                }

                Assert.AreEqual(false, block.GetValue(key2, out TestTransaction result2));
                Assert.AreEqual(default, result2);

                scope.Complete();

                Assert.AreEqual(true, block.GetValue(key1, out result1));
                Assert.AreEqual(true, block.GetValue(key2, out result2));
                Assert.AreEqual(data1, result1);
                Assert.AreEqual(data2, result2);
            }
        }

        [TestMethod]
        public void TestPersistentBlockTransactionFacade_GetSetValuesWithinScope2()
        {
            const string block1Key = @"BlockKey1";
            const string block2Key = @"BlockKey2";

            const string block1Name = @"TestBlock1";
            const string block2Name = @"TestBlock2";

            var random = new Random();
            var block1Data = TestTransaction.GetRandomObject(random);
            var block2Data = TestTransaction.GetRandomObject(random);

            var block1 = _persistenceProviderFacade.GetOrCreateBlock(block1Name, PersistenceLevel.Critical);
            var block2 = _persistenceProviderFacade.GetOrCreateBlock(block2Name, PersistenceLevel.Critical);

            using (var scope = _persistentStorageManager.ScopedTransaction())
            {
                using (var transaction = block1.Transaction())
                {
                    Assert.AreEqual(true, transaction.SetValue(block1Key, block1Data));
                    transaction.Commit();
                }

                Assert.AreEqual(false, block1.GetValue(block1Key, out TestTransaction block1Result));
                Assert.AreEqual(default, block1Result);

                using (var transaction = block2.Transaction())
                {
                    Assert.AreEqual(true, transaction.SetValue(block2Key, block2Data));
                    transaction.Commit();
                }

                Assert.AreEqual(false, block2.GetValue(block2Key, out TestTransaction block2Result));
                Assert.AreEqual(default, block2Result);

                scope.Complete();

                Assert.AreEqual(true, block1.GetValue(block1Key, out block1Result));
                Assert.AreEqual(true, block2.GetValue(block2Key, out block2Result));
                Assert.AreEqual(block1Data, block1Result);
                Assert.AreEqual(block2Data, block2Result);
            }
        }

        private SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(ConnectionString());
            //connection.SetPassword(DatabasePassword);
            return connection;
        }

        private string ConnectionString()
        {
            var sqlBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = _databaseFullPath,
                Pooling = true,
                Password = DatabasePassword,
            };

            return $"{sqlBuilder.ConnectionString};";
        }
    }
}
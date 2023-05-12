namespace Aristocrat.Monaco.Hardware.Tests.StorageAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using Microsoft.Data.Sqlite;
    using System.IO;
    using System.Linq;
    using Contracts.Persistence;
    using Hardware.Serial;
    using Hardware.StorageAdapters;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using System.Text;

    [TestClass]
    public class SqlSecondaryStorageManagerTest
    {
        private const string CreateTableCommand =
            "CREATE TABLE TestTable (Name TEXT PRIMARY KEY NOT NULL, Count INTEGER) WITHOUT ROWID";

        private const string DummyPrimaryDirectory = "Data";
        private const string DummySecondaryDirectory = "DataSecondary";

        private const string DbFileName = "NVRam.sqlite";
        private const string DbFilePassword = "tk7tjBLQ8GpySFNZTHYD";
        private readonly List<string> _nonDbFiles = new List<string> { "test1.txt", "test2.log" };

        private string _primaryDirectory;
        private string _secondaryDirectory;

        private SqlSecondaryStorageManager _target;

        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        [Timeout(5000)]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());

            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _propertiesManager =
                MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _eventBus =
                MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            EncodingProvider netFrameworkEncoding = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(netFrameworkEncoding);

            _target = new SqlSecondaryStorageManager();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();

            DeleteAll();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void VerifyIntegrityVerificationFailsWhenPrimaryRootNotSet()
        {
            CreateSecondaryDirectory();
            _target.SetPaths("", _secondaryDirectory);

            Assert.IsFalse(_target.Verify());
        }

        [TestMethod]
        public void VerifyIntegrityVerificationFailsWhenSecondaryRootNotSet()
        {
            CreatePrimaryDirectory();
            _target.SetPaths(_primaryDirectory, "");

            Assert.IsFalse(_target.Verify());
        }

        [TestMethod]
        public void VerifyMissingFilesAreCopiedToSecondaryStorage()
        {
            CreatePrimaryDirectory();
            var file1 = Path.Combine(_primaryDirectory, DbFileName);
            CreateDbFile(file1);
            CreateSecondaryDirectory();
            _target.SetPaths(_primaryDirectory, _secondaryDirectory);
            Assert.IsTrue(_target.Verify());
            Assert.IsTrue(IsFileSame(DbFileName));
            Assert.IsFalse(_nonDbFiles.Any(x => File.Exists(Path.Combine(_primaryDirectory, x))));
        }

        [TestMethod]
        public void VerifyOnlyExpectedFilesCopiedToPrimaryStorage()
        {
            CreateSecondaryDirectory();

            foreach (var file in _nonDbFiles)
            {
                CreateFiles(_secondaryDirectory, file);
            }

            var file2 = Path.Combine(_secondaryDirectory, DbFileName);
            CreateDbFile(file2);

            CreatePrimaryDirectory();

            _target.SetPaths(_primaryDirectory, _secondaryDirectory);
            Assert.IsTrue(_target.Verify());
            Assert.IsTrue(IsFileSame(DbFileName));
            Assert.IsFalse(_nonDbFiles.Any(x => File.Exists(Path.Combine(_primaryDirectory, x))));
        }

        [TestMethod]
        public void VerifyUnexpectedFilesThrows()
        {
            CreatePrimaryDirectory();

            foreach (var file in _nonDbFiles)
            {
                CreateFiles(_primaryDirectory, file, "test1");
            }

            CreateSecondaryDirectory();

            foreach (var file in _nonDbFiles)
            {
                CreateFiles(_secondaryDirectory, file, "test2");
            }

            _target.SetPaths(_primaryDirectory, _secondaryDirectory);

            Assert.ThrowsException<Exception>(() => _target.Verify());
        }

        [TestMethod]
        public void VerifyDbFilesAreVerified()
        {
            CreatePrimaryDirectory();
            CreateSecondaryDirectory();

            var file1 = Path.Combine(_primaryDirectory, DbFileName);
            var file2 = Path.Combine(_secondaryDirectory, DbFileName);

            CreateDbFile(file1);
            CreateDbFile(file2);

            _target.SetPaths(_primaryDirectory, _secondaryDirectory);

            Assert.IsTrue(_target.Verify());
        }

        [TestMethod]
        public void VerifyDbFilesAreRecoveredWhenOneIsCorrupted()
        {
            CreatePrimaryDirectory();
            CreateSecondaryDirectory();

            var file1 = Path.Combine(_primaryDirectory, DbFileName);
            var file2 = Path.Combine(_secondaryDirectory, DbFileName);

            CreateDbFile(file1);
            CreateDbFile(file2);

            // corrupt first file
            WriteToFile(file1, "test");

            var dateTime = DateTime.UtcNow;

            OverwriteLastWriteTime(file1, dateTime);
            OverwriteLastWriteTime(file2, dateTime);

            _target.SetPaths(_primaryDirectory, _secondaryDirectory);

            Assert.IsTrue(_target.Verify());
        }

        [TestMethod]
        public void VerifyIntegrityVerificationFailsWhenBothDbFilesCorrupted()
        {
            CreatePrimaryDirectory();
            CreateSecondaryDirectory();

            var file1 = Path.Combine(_primaryDirectory, DbFileName);
            var file2 = Path.Combine(_secondaryDirectory, DbFileName);

            CreateDbFile(file1);
            CreateDbFile(file2);

            // corrupt first file
            WriteToFile(file1, "test");

            // corrupt second file
            WriteToFile(file2, "test");

            var dateTime = DateTime.UtcNow;

            OverwriteLastWriteTime(file1, dateTime);
            OverwriteLastWriteTime(file2, dateTime);

            _target.SetPaths(_primaryDirectory, _secondaryDirectory);

            Assert.IsFalse(_target.Verify());
        }

        [TestMethod]
        public void VerifyErrorEventGeneratedWhenSecondaryMediaExpectedButNotConnected()
        {
            CreatePrimaryDirectory();

            _propertiesManager
                .Setup(p => p.GetProperty(SecondaryStorageConstants.SecondaryStorageRequired, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(SecondaryStorageConstants.MirrorRootKey, It.IsAny<string>()))
                .Returns(_secondaryDirectory);

            _eventBus.Setup(e => e.Publish(It.IsAny<SecondaryStorageErrorEvent>())).Verifiable();


            _target.SetPaths(_primaryDirectory, _secondaryDirectory);

            _target.VerifyConfiguration();

            _eventBus.Verify();

        }

        [TestMethod]
        public void VerifyErrorEventNotGeneratedWhenSecondaryMediaExpectedAndConnected()
        {
            CreatePrimaryDirectory();
            CreateSecondaryDirectory();

            _propertiesManager
                .Setup(p => p.GetProperty(SecondaryStorageConstants.SecondaryStorageRequired, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(SecondaryStorageConstants.MirrorRootKey, It.IsAny<string>()))
                .Returns(_secondaryDirectory);

            _eventBus.Setup(e => e.Publish(It.IsAny<SecondaryStorageErrorEvent>())).Throws(new Exception("Shouldn't raise storage error if secondary storage is required and connected"));


            _target.SetPaths(_primaryDirectory, _secondaryDirectory);

            _target.Verify();

            _target.VerifyConfiguration();

            _eventBus.Verify();

        }

        [TestMethod]
        public void VerifyErrorEventGeneratedWhenSecondaryMediaNotExpectedButConnected()
        {
            CreatePrimaryDirectory();
            CreateSecondaryDirectory();

            _propertiesManager
                .Setup(p => p.GetProperty(SecondaryStorageConstants.SecondaryStorageRequired, It.IsAny<bool>()))
                .Returns(false);
            _propertiesManager.Setup(p => p.GetProperty(SecondaryStorageConstants.MirrorRootKey, It.IsAny<string>()))
                .Returns(_secondaryDirectory);

            _eventBus.Setup(e => e.Publish(It.IsAny<SecondaryStorageErrorEvent>())).Verifiable();

            _target.SetPaths(_primaryDirectory, _secondaryDirectory);

            _target.VerifyConfiguration();

            _eventBus.Verify();

        }

        [TestMethod]
        public void VerifyErrorEventNotGeneratedWhenSecondaryMediaNotExpectedAndNotConnected()
        {
            CreatePrimaryDirectory();

            _propertiesManager
                .Setup(p => p.GetProperty(SecondaryStorageConstants.SecondaryStorageRequired, It.IsAny<bool>()))
                .Returns(false);
            _propertiesManager.Setup(p => p.GetProperty(SecondaryStorageConstants.MirrorRootKey, It.IsAny<string>()))
                .Returns(_secondaryDirectory);

            _eventBus.Setup(e => e.Publish(It.IsAny<SecondaryStorageErrorEvent>()))
                .Throws(
                    new Exception(
                        "Shouldn't raise storage error if secondary storage is not required and not connected"));

            _target.SetPaths(_primaryDirectory, _secondaryDirectory);

            _target.Verify();

            _target.VerifyConfiguration();

            _eventBus.Verify();

        }

        private static void WriteToFile(string filePath, string content)
        {
            SqliteConnection.ClearAllPools();

            using (var fs = File.OpenWrite(filePath))
            {
                if (!string.IsNullOrEmpty(content))
                {
                    fs.Write(content.ToByteArray(), 0, content.ToByteArray().Length);
                }
            }
        }

        private static void OverwriteLastWriteTime(string filePath, DateTime dateTime)
        {
            var fs = new FileInfo(filePath);
            if (fs.Exists)
            {
                fs.LastWriteTimeUtc = dateTime;
            }
        }

        private static bool FilesEquals(FileInfo f1, FileInfo f2)
        {
            if (f1 == null || f2 == null)
            {
                return false;
            }

            return f1.Name == f2.Name &&
                   f1.Length == f2.Length && FilesContentEqual(f1, f2);
        }

        private static bool FilesContentEqual(FileInfo f1, FileInfo f2)
        {
            using (var s1 = f1.OpenRead())
            {
                using (var s2 = f2.OpenRead())
                {
                    int file1Byte, file2Byte;
                    do
                    {
                        // Read one byte from each file.
                        file1Byte = s1.ReadByte();
                        file2Byte = s2.ReadByte();
                    } while (file1Byte == file2Byte && file1Byte != -1);

                    return file1Byte - file2Byte == 0;
                }
            }
        }

        private bool IsFileSame(string fileName)
        {
            SqliteConnection.ClearAllPools();
            return FilesEquals(
                new FileInfo(Path.Combine(_primaryDirectory, fileName)),
                new FileInfo(Path.Combine(_secondaryDirectory, fileName)));
        }

        private void CreateDbFile(string dbFilePath)
        {
            // Create dummy table
            using (var connection = CreateConnection(dbFilePath))
            {
                connection.Open();

                using (var command = new SqliteCommand(CreateTableCommand, connection))
                {
                    try
                    {
                        //command.CommandText = CreateTableCommand;
                        command.ExecuteNonQuery();
                       
                    }
                    catch (SqliteException)
                    {
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                
            }
        }

        private SqliteConnection CreateConnection(string filePath)
        {
            var connection = new SqliteConnection(ConnectionString(filePath));

            //connection.SetPassword(DbFilePassword);

            return connection;
        }

        private string ConnectionString(string filePath)
        {
            var sqlBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = filePath,
                Pooling = true,
                Password = DbFilePassword,
            };

            return $"{sqlBuilder.ConnectionString};";
        }

        private void CreatePrimaryDirectory()
        {
            try
            {
                var currentDirectory = Directory.GetCurrentDirectory();

                _primaryDirectory = Path.Combine(currentDirectory, DummyPrimaryDirectory);

                Directory.CreateDirectory(_primaryDirectory);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void CreateSecondaryDirectory()
        {
            try
            {
                var currentDirectory = Directory.GetCurrentDirectory();

                _secondaryDirectory = Path.Combine(currentDirectory, DummySecondaryDirectory);

                Directory.CreateDirectory(_secondaryDirectory);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void CreateFiles(string directory, string filename, string content = "ABC")
        {
            try
            {
                if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(filename))
                {
                    Assert.Inconclusive();
                }

                var filePath = Path.Combine(directory, filename);

                using (var fs = File.Create(filePath))
                {
                    if (!string.IsNullOrEmpty(content))
                    {
                        fs.Write(content.ToByteArray(), 0, content.ToByteArray().Length);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ClearSqlPoolConnection(string file)
        {
            using (var connection = CreateConnection(file))
            {
                connection.Close();
            }

            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void DeleteAll()
        {

            MoqServiceManager.RemoveInstance();
            if (Directory.Exists(_primaryDirectory))
            {
                foreach (var file in Directory.GetFiles(_primaryDirectory))
                {
                    ClearSqlPoolConnection(file);

                    File.Delete(file);
                }

                Directory.Delete(_primaryDirectory);
            }

            if (!Directory.Exists(_secondaryDirectory))
            {
                return;
            }

            {
                foreach (var file in Directory.GetFiles(_secondaryDirectory))
                {
                    ClearSqlPoolConnection(file);

                    File.Delete(file);
                }

                Directory.Delete(_secondaryDirectory);
            }
        }
    }
}
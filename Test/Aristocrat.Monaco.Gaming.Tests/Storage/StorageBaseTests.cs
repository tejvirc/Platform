using Aristocrat.Monaco.Hardware.Contracts;
using Aristocrat.Monaco.Hardware.Contracts.Persistence;
using Aristocrat.Monaco.Hardware.StorageAdapters;
using Aristocrat.Monaco.Kernel;
using Aristocrat.Monaco.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aristocrat.Monaco.Gaming.Tests.Storage
{
    [TestClass()]
    public class StorageBaseTests
    {
        private Mock<IPathMapper> _pathMapper;
        private Mock<IEventBus> _eventBus;
        private GameStorageManager _gameStorageManager;
        private Mock<IPropertiesManager> _propertiesManager;

        private string _dbDirectory;

        [TestInitialize()]
        public void Initialize()
        {
            CreateEmptyDatabaseDirectory();

            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _pathMapper =
                MoqServiceManager.CreateAndAddService<IPathMapper>(MockBehavior.Strict);

            _eventBus =
                MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _propertiesManager =
                MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            var pathMapper = ServiceManager.GetInstance().GetService<IPathMapper>();
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();


            var name = "StorageTest.sqlite";
            var password = "SesamOpen";

            _pathMapper.Setup(m => m.GetDirectory(It.Is<string>(s => s == HardwareConstants.DataPath)))
                .Returns(new DirectoryInfo(_dbDirectory));

            _eventBus.Setup(m => m.Subscribe<PersistentStorageClearReadyEvent>(It.IsAny<object>(), It.IsAny<Action<PersistentStorageClearReadyEvent>>()))
                .Callback<object, Action<PersistentStorageClearReadyEvent>>((o, a) => { });

            _propertiesManager.Setup(p => p.GetProperty(SecondaryStorageConstants.MirrorRootKey, It.IsAny<string>()))
            .Returns("");

            _propertiesManager.Setup(p => p.SetProperty(SecondaryStorageConstants.MirrorRootKey, It.IsAny<string>()));

            var persistentManager = new SqlPersistentStorageManager(pathMapper, eventBus, name, password, false);

            _gameStorageManager = new GameStorageManager(persistentManager);
        }

        private void CreateEmptyDatabaseDirectory()
        {
            try
            {
                var currentDirectory = Directory.GetCurrentDirectory();

                _dbDirectory = Path.Combine(currentDirectory, HardwareConstants.DataPath.Replace("/", "Test_"));
                RemoveDirectoryContent(_dbDirectory);
                Directory.CreateDirectory(_dbDirectory);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void RemoveDirectoryContent(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    //Delete all files and directories from the Directory
                    foreach (string path in Directory.GetDirectories(directory))
                    {
                        RemoveDirectoryContent(path);
                        Directory.Delete(path);
                    }

                    foreach (string path in Directory.GetFiles(directory))
                    {
                        File.Delete(path);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static List<(int GameId, string Name, string Value)> CreateTestDataWithGameIdName()
        {
            return new List<(int GameId, string Name, string Value)>()
            {
                (1,"StorageName1", "Simple_Value_1_Name1"),
                (1,"StorageName1", "Simple_Value_1_Name1"),
                (2,"StorageName1", "Simple_Value_2_Name1"),
                (2,"StorageName1", "Simple_Value_2_Name1"),

                (1,"StorageName2", "Simple_Value_1_Name2"),
                (1,"StorageName2", "Simple_Value_1_Name2"),
                (2,"StorageName2", "Simple_Value_2_Name2"),
                (2,"StorageName2", "Simple_Value_2_Name2"),
            };
        }

        private static List<(int GameId, long BetAmount, string Name, string Value)> CreateTestDataWithGameIdBetName()
        {
            return new List<(int GameId, long BetAmount, string Name, string Value)>()
            {
                (1, 20, "StorageName1", "Simple_Value_1_20_Name1"),
                (1, 50, "StorageName1", "Simple_Value_1_50_Name1"),
                (2, 20, "StorageName1", "Simple_Value_2_20_Name1"),
                (2, 50, "StorageName1", "Simple_Value_2_50_Name1"),

                (1, 20, "StorageName2", "Simple_Value_1_20_Name2"),
                (1, 50, "StorageName2", "Simple_Value_1_50_Name2"),
                (2, 20, "StorageName2", "Simple_Value_2_20_Name2"),
                (2, 50, "StorageName2", "Simple_Value_2_50_Name2"),
            };
        }

        private static List<(int GameId, long BetAmount, string Name, string KeyName, string Value)> CreateTestDataWithGameIdBetNameKeyValue()
        {
            return new List<(int GameId, long BetAmount, string Name, string KeyName, string Value)>()
            {
                (1, 20, "StorageName1", "KeyName1", "Value_1_20_Name1_KeyName1"),
                (1, 50, "StorageName1", "KeyName1", "Value_1_50_Name1_KeyName1"),
                (2, 20, "StorageName1", "KeyName1", "Value_2_20_Name1_KeyName1"),
                (2, 50, "StorageName1", "KeyName1", "Value_2_50_Name1_KeyName1"),

                (1, 20, "StorageName2", "KeyName1", "Value_1_20_Name2_KeyName1"),
                (1, 50, "StorageName2", "KeyName1", "Value_1_50_Name2_KeyName1"),
                (2, 20, "StorageName2", "KeyName1", "Value_2_20_Name2_KeyName1"),
                (2, 50, "StorageName2", "KeyName1", "Value_2_50_Name2_KeyName1"),

                (1, 20, "StorageName1", "My1KeyName1", "Value_1_20_Name1_My1KeyName1"),
                (1, 50, "StorageName1", "My1KeyName1", "Value_1_50_Name1_My1KeyName1"),
                (2, 20, "StorageName1", "My1KeyName1", "Value_2_20_Name1_My1KeyName1"),
                (2, 50, "StorageName1", "My1KeyName1", "Value_2_50_Name1_My1KeyName1"),

                (1, 20, "StorageName2", "2KeyName", "Value_1_20_Name2_2KeyName2"),
                (1, 50, "StorageName2", "2KeyName", "Value_1_50_Name2_2KeyName2"),
                (2, 20, "StorageName2", "2KeyName", "Value_2_20_Name2_2KeyName2"),
                (2, 50, "StorageName2", "2KeyName", "Value_2_50_Name2_2KeyName2"),
            };
        }

        private static List<(string Name, string KeyName, string Value)> CreateTestDataWithKey()
        {
            return new List<(string Name, string KeyName, string Value)>()
            {
                ("MyStorageName1", "Key1", "Value_MyStorageName1_Key1"),
                ("MyStorageName1", "Key2", "Value_MyStorageName1_Key2"),
                ("MyStorageName1", "Key3", "Value_MyStorageName1_Key3"),
                ("MyStorageName2", "Key1", "Value_MyStorageName2_Key1"),
                ("MyStorageName2", "Key2", "Value_MyStorageName2_Key2"),
                ("MyStorageName2", "Key3", "Value_MyStorageName2_Key3"),
                ("MyStorageName3", "Key1", "Value_MyStorageName3_Key1"),
                ("MyStorageName3", "Key2", "Value_MyStorageName3_Key2"),
                ("MyStorageName3", "Key3", "Value_MyStorageName3_Key3")
            };
        }

        private static List<(string Name, string Value)> CreateTestData()
        {
            return new List<(string Name, string Value)>()
            {
                ("MyStorageName1", "Value_MyStorageName1"),
                ("MyStorageName2", "Value_MyStorageName2"),
                ("MyStorageName3", "Value_MyStorageName3"),
            };
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod()]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_gameStorageManager);
        }

        [TestMethod()]
        public void GetValueFalseTest()
        {
            var items = CreateTestDataWithGameIdBetName();

            //foreach (var keyValuePair in items)
            //{
            //    _gameStorageManager.SetValue(keyValuePair.GameId,
            //        keyValuePair.Name, keyValuePair.Value);
            //}

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                IEnumerable<char> output = null;
                var exists = _gameStorageManager.TryGetValues<char>(keyValuePair.GameId + 99,
                    keyValuePair.Name, out output);

                Assert.IsFalse(exists);
                Assert.AreNotEqual(keyValuePair.Value, string.Join("", output));
            }
        }


        [TestMethod()]
        public void TryGetValuesGameIdName()
        {
            var items = CreateTestDataWithGameIdName();

            foreach (var keyValuePair in items)
            {
                _gameStorageManager.SetValue(keyValuePair.GameId,
                    keyValuePair.Name, keyValuePair.Value);
            }

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var value = _gameStorageManager.GetValues<char>(keyValuePair.GameId,
                    keyValuePair.Name);

                Assert.AreEqual(keyValuePair.Value, string.Join("", value));
            }

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                IEnumerable<char> output = null;
                var exists = _gameStorageManager.TryGetValues<char>(keyValuePair.GameId,
                    keyValuePair.Name, out output);

                Assert.IsTrue(exists);
                Assert.AreEqual(keyValuePair.Value, string.Join("", output));
            }
        }

        [TestMethod()]
        public void TryGetValues()
        {
            var items = CreateTestDataWithGameIdBetName();

            foreach (var keyValuePair in items)
            {
                _gameStorageManager.SetValue(keyValuePair.GameId, keyValuePair.BetAmount,
                    keyValuePair.Name, keyValuePair.Value);
            }

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var value = _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                    keyValuePair.BetAmount, keyValuePair.Name);

                Assert.AreEqual(keyValuePair.Value, value);
            }

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                IEnumerable<char> output = null;
                var exists = _gameStorageManager.TryGetValues<char>(keyValuePair.GameId,
                    keyValuePair.BetAmount, keyValuePair.Name, out output);

                Assert.IsTrue(exists);
                Assert.AreEqual(keyValuePair.Value, string.Join("", output));
            }
        }

        [TestMethod()]
        public void TryGetValuesWithKey()
        {
            var items = CreateTestDataWithGameIdBetNameKeyValue();

            foreach (var keyValuePair in items)
            {
                _gameStorageManager.SetValue(keyValuePair.GameId, keyValuePair.BetAmount,
                    keyValuePair.Name, keyValuePair.KeyName, keyValuePair.Value);
            }

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var value = _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                    keyValuePair.BetAmount, keyValuePair.Name, keyValuePair.KeyName);

                Assert.AreEqual(keyValuePair.Value, value);
            }

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                IEnumerable<char> output = null;
                var exists = _gameStorageManager.TryGetValues<char>(keyValuePair.GameId,
                    keyValuePair.BetAmount, keyValuePair.Name, keyValuePair.KeyName, out output);

                Assert.IsTrue(exists);
                Assert.AreEqual(keyValuePair.Value, string.Join("", output));
            }
        }

        [TestMethod()]
        public void SetValuePlainTest()
        {
            var items = CreateTestData();

            foreach (var keyValuePair in items)
            {
                _gameStorageManager.SetValue(keyValuePair.Name, keyValuePair.Value);
            }

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var value = _gameStorageManager.GetValue<string>(keyValuePair.Name);

                Assert.AreEqual(keyValuePair.Value, value);
            }
        }

        [TestMethod()]
        public void SetValuePlainWithKey()
        {
            var items = CreateTestDataWithKey();

            foreach (var keyValuePair in items)
            {
                _gameStorageManager.SetValue(keyValuePair.Name, keyValuePair.KeyName, keyValuePair.Value);
            }

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var value = _gameStorageManager.GetValue<string>(keyValuePair.Name, keyValuePair.KeyName);

                Assert.AreEqual(keyValuePair.Value, value);
            }
        }

        [TestMethod()]
        public void SetValueWithGameIdBetName()
        {
            var items = CreateTestDataWithGameIdBetName();

            foreach (var keyValuePair in items)
            {
                _gameStorageManager.SetValue(keyValuePair.GameId, keyValuePair.BetAmount,
                    keyValuePair.Name, keyValuePair.Value);
            }

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var value = _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                    keyValuePair.BetAmount, keyValuePair.Name);

                Assert.AreEqual(keyValuePair.Value, value);
            }
        }

        [TestMethod()]
        public void SetValueWithGameIdBetNameKeyName()
        {
            var items = CreateTestDataWithGameIdBetNameKeyValue();

            foreach (var keyValuePair in items)
            {
                _gameStorageManager.SetValue(keyValuePair.GameId, keyValuePair.BetAmount,
                    keyValuePair.Name, keyValuePair.KeyName, keyValuePair.Value);
            }

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var value = _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                    keyValuePair.BetAmount, keyValuePair.Name, keyValuePair.KeyName);

                Assert.AreEqual(keyValuePair.Value, value);
            }
        }

        [TestMethod()]
        public void ClearAllValuesWithKeyNameTest()
        {
            var items = CreateTestDataWithGameIdBetNameKeyValue();

            // create internal function to set the items
            var fillDB = new Action(() =>
            {
                foreach (var keyValuePair in items)
                {
                    _gameStorageManager.SetValue(keyValuePair.GameId,
                        keyValuePair.BetAmount,
                        keyValuePair.Name,
                        keyValuePair.KeyName,
                        keyValuePair.Value);
                }
            });

            fillDB();

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var valueBefore = _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                    keyValuePair.BetAmount,
                    keyValuePair.Name,
                    keyValuePair.KeyName);
                Assert.AreEqual(keyValuePair.Value, valueBefore);
            }

            // get all gameIds of items
            // Check whether all items are deleted correctly and verify whether the others are still there.
            var gameIds = items.Select(x => x.GameId).Distinct().ToList();

            foreach (var gameId in gameIds)
            {
                fillDB(); // refill the Database with all data
                foreach (var keyValuePair in items.Where(x => x.GameId == gameId))
                {
                    _gameStorageManager.ClearAllValuesWithKeyName(keyValuePair.GameId, keyValuePair.BetAmount,
                                           keyValuePair.Name);

                    var valueAfter = _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                            keyValuePair.BetAmount, keyValuePair.Name, keyValuePair.KeyName);

                    Assert.AreNotEqual(keyValuePair.Value, valueAfter);
                }

                foreach (var keyValuePair in items.Where(x => x.GameId != gameId))
                {
                    var valueAfter = _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                            keyValuePair.BetAmount, keyValuePair.Name, keyValuePair.KeyName);

                    Assert.AreEqual(keyValuePair.Value, valueAfter);
                }
            }
        }

        [TestMethod()]
        public void ClearAllValuesWithKeyNameTestKeyTripleDeletions()
        {
            var items = CreateTestDataWithGameIdBetNameKeyValue();

            // create internal function to set the items
            var fillDB = new Action(() =>
            {
                foreach (var keyValuePair in items)
                {
                    _gameStorageManager.SetValue(keyValuePair.GameId,
                        keyValuePair.BetAmount,
                        keyValuePair.Name,
                        keyValuePair.KeyName,
                        keyValuePair.Value);
                }
            });

            fillDB();

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var valueBefore = _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                    keyValuePair.BetAmount,
                    keyValuePair.Name,
                    keyValuePair.KeyName);
                Assert.AreEqual(keyValuePair.Value, valueBefore);
            }

            var distinctData = items
                .GroupBy(data => new { data.GameId, data.BetAmount, data.Name })
                .Select(group => (group.Key.GameId, group.Key.BetAmount, group.Key.Name))
                .ToList();

            fillDB();

            //Iterate over all distinctData and check whether the clear is succesful and has not influenced other data
            foreach (var data in distinctData)
            {
                fillDB();
                {
                    var keyValuePair = distinctData.Where(x => x == data).FirstOrDefault();

                    _gameStorageManager.ClearAllValuesWithKeyName(keyValuePair.GameId, keyValuePair.BetAmount,
                                           keyValuePair.Name);

                    // Select all unique keys of the data set
                    var distinctKeys = items.Where(m => keyValuePair.GameId == m.GameId
                                && keyValuePair.BetAmount == m.BetAmount
                                && keyValuePair.Name == m.Name)
                            .Select(x => x.KeyName).Distinct().ToList();

                    // Checks whether all keys are cleared
                    foreach (var key in distinctKeys)
                    {
                        var value = _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                            keyValuePair.BetAmount, keyValuePair.Name, key);

                        Assert.AreEqual(value?.Count(), 0);
                    }
                }

                foreach (var keyValuePair in distinctData.Where(x => x != data))
                {
                    // Select all unique keys of the data set
                    var distinctKeys = items.Where(m => keyValuePair.GameId == m.GameId
                                && keyValuePair.BetAmount == m.BetAmount
                                && keyValuePair.Name == m.Name)
                            .Select(x => x.KeyName).Distinct().ToList();

                    // Checks whether all keys are not cleared for the specifc data set
                    foreach (var key in distinctKeys)
                    {
                        var value = _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                            keyValuePair.BetAmount, keyValuePair.Name, key);

                        Assert.AreNotEqual(value?.Count(), 0);
                    }
                }
            }
        }

        [TestMethod()]
        public void ClearAllValuesWithKeyNameTestDeletions()
        {
            var items = CreateTestDataWithKey();

            // create internal function to set the items
            var fillDB = new Action(() =>
            {
                foreach (var keyValuePair in items)
                {
                    _gameStorageManager.SetValue(
                        keyValuePair.Name,
                        keyValuePair.KeyName,
                        keyValuePair.Value);
                }
            });

            fillDB();

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var valueBefore = _gameStorageManager.GetValue<string>(
                    keyValuePair.Name,
                    keyValuePair.KeyName);
                Assert.AreEqual(keyValuePair.Value, valueBefore);
            }

            var distinctData = items
                .GroupBy(data => new { data.Name })
                .Select(group => (group.Key.Name))
                .ToList();

            fillDB();

            //Iterate over all distinctData and check whether the clear is succesful and has not influenced other data
            foreach (var data in distinctData)
            {
                fillDB();
                {
                    var keyValuePair = distinctData.Where(x => x == data).FirstOrDefault();

                    _gameStorageManager.ClearAllValuesWithKeyName(keyValuePair);

                    // Select all unique keys of the data set
                    var distinctKeys = items.Where(m => keyValuePair == m.Name)
                            .Select(x => x.KeyName).Distinct().ToList();

                    // Checks whether all keys are cleared
                    foreach (var key in distinctKeys)
                    {
                        var value = _gameStorageManager.GetValue<string>(keyValuePair, key);

                        Assert.AreEqual(value?.Count(), 0);
                    }
                }

                foreach (var keyValuePair in distinctData.Where(x => x != data))
                {
                    // Select all unique keys of the data set
                    var distinctKeys = items.Where(m => keyValuePair == m.Name)
                            .Select(x => x.KeyName).Distinct().ToList();

                    // Checks whether all keys are not cleared for the specifc data set
                    foreach (var key in distinctKeys)
                    {
                        var value = _gameStorageManager.GetValue<string>(keyValuePair, key);

                        Assert.AreNotEqual(value?.Count(), 0);
                    }
                }
            }
        }

        [TestMethod()]
        public void GetKeyNameAndValuesWithGameIdBetNameKeyValue()
        {
            var items = CreateTestDataWithGameIdBetNameKeyValue();

            // create internal function to set the items
            var fillDB = new Action(() =>
            {
                foreach (var keyValuePair in items)
                {
                    _gameStorageManager.SetValue(keyValuePair.GameId,
                        keyValuePair.BetAmount,
                        keyValuePair.Name,
                        keyValuePair.KeyName,
                        keyValuePair.Value);
                }
            });

            fillDB();

            //test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var valueBefore = _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                    keyValuePair.BetAmount,
                    keyValuePair.Name,
                    keyValuePair.KeyName);
                Assert.AreEqual(keyValuePair.Value, valueBefore);
            }

            var distinctData = items
                .GroupBy(data => new { data.GameId, data.BetAmount, data.Name })
                .Select(group => (group.Key.GameId, group.Key.BetAmount, group.Key.Name))
                .ToList();

            Assert.IsTrue(distinctData.Count() > 0);

            foreach (var data in distinctData)
            {
                var receivedKeyValuePairs = _gameStorageManager.GetKeyNameAndValues(data.GameId, data.BetAmount, data.Name);
                var dataKeyValuePairs = items.Where(m => data.GameId == m.GameId
                        && data.BetAmount == m.BetAmount
                        && data.Name == m.Name).Select(group => (group.KeyName, group.Value)).ToList();

                foreach (var dataPair in dataKeyValuePairs)
                {
                    Assert.AreEqual(dataPair.Value, receivedKeyValuePairs[dataPair.KeyName]);
                }

                foreach (var receivedPair in receivedKeyValuePairs)
                {
                    string dataValue = dataKeyValuePairs.Where(x => x.KeyName == receivedPair.Key).Select(value => value.Value).FirstOrDefault();
                    Assert.AreEqual(receivedPair.Value, dataValue);
                }
            }
        }

        [TestMethod()]
        public void GetKeyNameAndValuesTest()
        {
            var items = CreateTestDataWithKey();

            // Internal function to set the items
            var fillDB = new Action(() =>
            {
                foreach (var keyValuePair in items)
                {
                    _gameStorageManager.SetValue(
                        keyValuePair.Name,
                        keyValuePair.KeyName,
                        keyValuePair.Value);
                }
            });

            fillDB();

            //Test whether the values are set in the database
            foreach (var keyValuePair in items)
            {
                var valueBefore = _gameStorageManager.GetValue<string>(
                    keyValuePair.Name,
                    keyValuePair.KeyName);
                Assert.AreEqual(keyValuePair.Value, valueBefore);
            }

            // find distinct Data
            var distinctData = items.Select(x => x.Name).Distinct().ToList();
            Assert.IsTrue(distinctData.Count() > 0);

            foreach (var data in distinctData)
            {
                var receivedKeyValuePairs = _gameStorageManager.GetKeyNameAndValues(data);
                var dataKeyValuePairs = items.Where(m => data == m.Name).Select(group => (group.KeyName, group.Value)).ToList();

                foreach (var dataPair in dataKeyValuePairs)
                {
                    Assert.AreEqual(dataPair.Value, receivedKeyValuePairs[dataPair.KeyName]);
                }

                foreach (var receivedPair in receivedKeyValuePairs)
                {
                    string dataValue = dataKeyValuePairs.Where(x => x.KeyName == receivedPair.Key).Select(value => value.Value).FirstOrDefault();
                    Assert.AreEqual(receivedPair.Value, dataValue);
                }
            }
        }

        [TestMethod()]
        public void MixedModeReadTest()
        {
            var test_data_with_key = CreateTestDataWithKey();
            test_data_with_key.ForEach(keyValuePair =>
                _gameStorageManager.SetValue(keyValuePair.Name, keyValuePair.KeyName, keyValuePair.Value));

            var test_data_with_game_id_name = CreateTestDataWithGameIdName();
            test_data_with_game_id_name.ForEach(keyValuePair =>
                _gameStorageManager.SetValue(keyValuePair.GameId, keyValuePair.Name, keyValuePair.Value));

            var test_data_with_game_id_bet_name = CreateTestDataWithGameIdBetName();
            test_data_with_game_id_bet_name.ForEach(keyValuePair =>
                _gameStorageManager.SetValue(keyValuePair.GameId, keyValuePair.BetAmount,
                    keyValuePair.Name, keyValuePair.Value));

            var test_data_with_game_id_bet_name_key_value = CreateTestDataWithGameIdBetNameKeyValue();
            test_data_with_game_id_bet_name_key_value.ForEach(keyValuePair =>
                _gameStorageManager.SetValue(keyValuePair.GameId, keyValuePair.BetAmount,
                    keyValuePair.Name, keyValuePair.KeyName, keyValuePair.Value));

            var plain_test_data = CreateTestData();

            plain_test_data.ForEach(keyValuePair =>
                _gameStorageManager.SetValue(keyValuePair.Name, keyValuePair.Value));

            test_data_with_key.ForEach(keyValuePair =>
                Assert.AreEqual(keyValuePair.Value, _gameStorageManager.GetValue<string>(keyValuePair.Name, keyValuePair.KeyName)));
            test_data_with_game_id_name.ForEach(keyValuePair =>
                Assert.AreEqual(keyValuePair.Value,
                    string.Join("", _gameStorageManager.GetValues<char>(keyValuePair.GameId, keyValuePair.Name))));
            test_data_with_game_id_bet_name.ForEach(keyValuePair =>
                Assert.AreEqual(keyValuePair.Value,
                    _gameStorageManager.GetValue<string>(keyValuePair.GameId, keyValuePair.BetAmount, keyValuePair.Name)));

            test_data_with_game_id_bet_name_key_value.ForEach(keyValuePair =>
                Assert.AreEqual(keyValuePair.Value, _gameStorageManager.GetValue<string>(keyValuePair.GameId,
                    keyValuePair.BetAmount, keyValuePair.Name, keyValuePair.KeyName)));

            plain_test_data.ForEach(keyValuePair =>
                Assert.AreEqual(keyValuePair.Value, _gameStorageManager.GetValue<string>(keyValuePair.Name)));
        }

        [TestMethod()]
        public void TestWithAllData()
        {
            MixedModeReadTest();
            GetKeyNameAndValuesWithGameIdBetNameKeyValue();
            GetKeyNameAndValuesTest();
        }
    }
}
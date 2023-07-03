namespace Aristocrat.Monaco.Hardware.StorageAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Aristocrat.Monaco.Common;
    using Cabinet.Contracts;
    using Contracts.Persistence;
    using Kernel;
    using log4net;
    using Microsoft.Data.Sqlite;

    public class SqlSecondaryStorageManager : ISecondaryStorageManager, IService
    {
        private const string TempFileName = "test.tmp";
        private const string G2SDbFileName = @"protocol.sqlite";
        private const string ProtocolDatabaseFiles = @"Database_.*\.sqlite";

        private static readonly Regex PlatformDatabaseRegex = new(
            $"^{ProtocolDatabaseFiles}$|^{Regex.Escape(G2SDbFileName)}$|^{Regex.Escape(StorageConstants.DatabaseFileName)}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private string _primaryPath;
        private string _secondaryPath;
        private bool _verified;
        private bool _initialized;

        public void SetPaths(string primary, string secondary)
        {
            if (_initialized)
            {
                return;
            }

            _primaryPath = primary;
            _secondaryPath = secondary;

            _initialized = true;
        }

        public bool Verify()
        {
            if (!_initialized)
            {
                return false;
            }

            if (!VerifySecondaryStorageIntegrity())
            {
                Logger.Debug("Secondary storage integrity check failed");

                _verified = false;

                return false;
            }

            _verified = true;

            Logger.Debug("Initialized mirror(storage).");

            return true;
        }

        public void VerifyConfiguration()
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            var required = properties.GetValue(SecondaryStorageConstants.SecondaryStorageRequired, false);

            var pathSet = !string.IsNullOrEmpty(
                properties.GetValue(SecondaryStorageConstants.MirrorRootKey, string.Empty));

            if (!required && pathSet)
            {
                // Secondary storage not supported, but connected
                eventBus.Publish(new SecondaryStorageErrorEvent(SecondaryStorageError.NotExpectedButConnected));

                return;
            }

            if (!required)
            {
                return;
            }

            // Secondary storage is required, verify whether it's connected and integrity
            if (!pathSet || !IsMediaConnected())
            {
                // Secondary storage required and missing.
                eventBus.Publish(new SecondaryStorageErrorEvent(SecondaryStorageError.ExpectedButNotConnected));
            }
            else if (!_verified)
            {
                // If integrity of secondary storage cannot be verified, raise storage error
                eventBus.Publish(new StorageErrorEvent(StorageError.InvalidHandle));
            }
        }

        /// <inheritdoc />
        public string Name => nameof(ISecondaryStorageManager);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ISecondaryStorageManager) };

        public void Initialize()
        {
        }

        public static string GetMirrorPath(string primaryStorageRoot)
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var mirrorRoot = properties.GetValue(StorageConstants.MirrorRootKey, string.Empty);

            mirrorRoot = GetSecondaryMediaRoot(primaryStorageRoot, mirrorRoot);

            properties.SetProperty(StorageConstants.MirrorRootKey, mirrorRoot);

            return mirrorRoot;
        }

        private static string GetSecondaryMediaRoot(string primaryStorageRoot, string currentMirrorRoot)
        {
            if (string.Equals(primaryStorageRoot, currentMirrorRoot, StringComparison.CurrentCultureIgnoreCase))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(currentMirrorRoot))
            {
                var parentMirror = Path.GetDirectoryName(currentMirrorRoot);

                if (Directory.Exists(currentMirrorRoot) ||
                    !string.IsNullOrEmpty(parentMirror) && !Directory.Exists(parentMirror))
                {
                    var dirInfo = Directory.CreateDirectory(currentMirrorRoot);

                    return dirInfo.Exists ? currentMirrorRoot : null;
                }
            }

            if (HardwareFamilyIdentifier.Identify() == HardwareFamily.Unknown)
            {
                return null;
            }

            var primary = Path.GetPathRoot(primaryStorageRoot);

            var diskInfo = DiskDeviceInfo.GetPhysicalDiskInfo().Where(
                x => x.Value.Any(y => y.DriveType == DriveType.Fixed) &&
                     x.Value.All(y => y.RootDirectory.Name != primary)).ToList();

            var driveInfo = diskInfo.FirstOrDefault().Value?.FirstOrDefault();
            if (driveInfo == null)
            {
                return null;
            }

            var mirrorPath = primaryStorageRoot.Replace(primary, driveInfo.RootDirectory.FullName);

            var tempFile = Path.Combine(driveInfo.RootDirectory.FullName, TempFileName);

            try
            {
                if (!Directory.Exists(mirrorPath))
                {
                    Directory.CreateDirectory(mirrorPath);
                }

                File.WriteAllText(tempFile, string.Empty);
            }
            catch (Exception e)
            {
                // failed to write to secondary media.
                Logger.Error("Failed to write to secondary drive", e);
                return null;
            }

            finally
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to write to secondary drive", e);
                    //ignore
                }
            }

            return mirrorPath;
        }

        private static string ConnectionString(string filePath, string password)
        {
            var sqlBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = filePath,
                Pooling = true,
                Password = password,
                DefaultTimeout = 15
            };

            return $"{sqlBuilder.ConnectionString};";
        }

        private static bool IsValidSqlFile(string filePath, string password)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                using var connection = CreateConnection(filePath, password);
                using var benchmarck = new Benchmark(nameof(IsValidSqlFile));
                connection.Open();

                // This command is used to verify that the SQL lite file is valid and does not have errors
                const string verifySqlFileIsValidCommand = "PRAGMA integrity_check(1)";
                const string successfulCommandResponse = "ok";
                using var command = new SqliteCommand(verifySqlFileIsValidCommand, connection);
                return command.ExecuteScalar().ToString() == successfulCommandResponse;
            }
            catch (Exception e)
            {
                Logger.Error("Integrity check failed for mirror", e);

                return false;
            }
        }

        private static SqliteConnection CreateConnection(string filePath, string password)
        {
            return new SqliteConnection(ConnectionString(filePath, password));
        }

        private bool VerifySqlFiles(string sqlFile)
        {
            var first = new FileInfo(Path.Combine(_primaryPath, sqlFile));
            var second = new FileInfo(Path.Combine(_secondaryPath, sqlFile));

            if (!first.Exists && !second.Exists)
            {
                return true;
            }

            if (ValidateSqlFiles(first, second) || ValidateSqlFiles(second, first))
            {
                return true;
            }

            Logger.Error($"{sqlFile} is corrupt");

            return false;

            bool ValidateSqlFiles(FileSystemInfo primary, FileSystemInfo secondary)
            {
                if (!IsValidSqlFile(
                    primary.FullName,
                    StorageConstants.DatabasePassword))
                {
                    return false;
                }

                Logger.Debug($"Restoring to primary {primary.FullName} => {secondary.FullName}.");

                SqliteConnection.ClearAllPools();

                File.Copy(primary.FullName, secondary.FullName, true);

                var sqliteTempFileExtensions = new[] { "-wal", "-shm", "-journal" };

                return sqliteTempFileExtensions.All(
                    x =>
                    {
                        var secondaryTempFile = secondary.FullName + x;

                        if (File.Exists(secondaryTempFile))
                        {
                            File.Delete(secondaryTempFile);
                        }

                        return true;
                    });
            }
        }

        private bool VerifySecondaryStorageIntegrity()
        {
            if (!IsMediaConnected())
            {
                return false;
            }

            var result = false;
            var allowedDatabases = Directory.GetFiles(_primaryPath)
                .Select(Path.GetFileName)
                .Where(x => PlatformDatabaseRegex.IsMatch(x))
                .Concat(Directory.GetFiles(_secondaryPath)
                    .Select(Path.GetFileName)
                    .Where(x => PlatformDatabaseRegex.IsMatch(x)))
                .Distinct()
                .ToList();

            try
            {
                result = allowedDatabases.All(VerifySqlFiles);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                return result;
            }

            var unexpectedFiles = Directory.GetFiles(_primaryPath)
                .Select(Path.GetFileName)
                .Except(allowedDatabases).ToList();

            if (unexpectedFiles.Any())
            {
                throw new Exception(
                    $"Unexpected files found in the primary media: {string.Join(",", unexpectedFiles)}");
            }

            return result;
        }

        private bool IsMediaConnected()
        {
            if (!_initialized || string.IsNullOrEmpty(_primaryPath) ||
                string.IsNullOrEmpty(_secondaryPath))
            {
                return false;
            }

            return Directory.Exists(_secondaryPath);
        }

        private static class DiskDeviceInfo
        {
            public static IDictionary<string, IEnumerable<DriveInfo>> GetPhysicalDiskInfo()
            {
                return GetPhysicalDevices().ToDictionary(x => x, x => GetPartitions(x).SelectMany(GetLogicalDisks));
            }

            private static IEnumerable<string> RunQuery(string query, string field)
            {
                using var searcher = new ManagementObjectSearcher(query);
                return searcher.Get().Cast<ManagementObject>().Select(x => x[field].ToString());
            }

            private static IEnumerable<string> GetPhysicalDevices()
            {
                return RunQuery("SELECT * FROM Win32_DiskDrive", "DeviceID");
            }

            private static IEnumerable<string> GetPartitions(string physicalDiskId)
            {
                return RunQuery(
                    $"Associators of {{Win32_DiskDrive.DeviceID='{physicalDiskId}'}} where AssocClass=Win32_DiskDriveToDiskPartition",
                    "DeviceID");
            }

            private static IEnumerable<DriveInfo> GetLogicalDisks(string partitionId)
            {
                var result = RunQuery(
                    $"Associators of {{Win32_DiskPartition.DeviceID='{partitionId}'}} where AssocClass=Win32_LogicalDiskToPartition",
                    "Name");

                return result.Join(DriveInfo.GetDrives(), x => x, y => y.Name.TrimEnd('\\'), (_, y) => y);
            }
        }
    }
}
namespace Aristocrat.Monaco.Protocol.Common.Installer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using Application.Contracts.Localization;
    using Monaco.Common.Exceptions;
    using Monaco.Common.Storage;
    using ICSharpCode.SharpZipLib.Zip;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using log4net;
    using PackageManifest;
    using PackageManifest.Models;

    /// <summary>
    ///     An implementation of <see cref="IInstallerService" />
    /// </summary>
    public class InstallerService : IInstallerService
    {
        private const int ArchiveFileCount = 2;

        // Yes, it's correct. It's a carry over from GEN8
        private static readonly byte[] Secret = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string DownloadsDirectoryPath = "/Downloads";
        private const string TemporaryDirectoryPath = @"temp";
        private const string StorageDirectoryPath = "/Packages";
        private const string ManifestExtension = "manifest";
        private const string Printer = @"printer";
        private const string NoteAcceptor = @"noteacceptor";

        private readonly IInstallerFactory _installerFactory;
        private readonly IPathMapper _pathMapper;
        private readonly IManifest<Image> _manifestService;
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly IPackageService _packageService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InstallerService" /> class.
        /// </summary>
        public InstallerService(
            IInstallerFactory installerFactory,
            IPathMapper pathMapper,
            IManifest<Image> manifestService,
            IFileSystemProvider fileSystemProvider,
            IPackageService packageService)
        {
            _installerFactory = installerFactory ?? throw new ArgumentNullException(nameof(installerFactory));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _manifestService = manifestService ?? throw new ArgumentNullException(nameof(manifestService));
            _fileSystemProvider = fileSystemProvider ?? throw new ArgumentNullException(nameof(fileSystemProvider));
            _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IInstallerService) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public (Image manifest, string path, long size, bool deviceChanged, ExitAction? action) InstallPackage(
            string packageId,
            Action updateAction = null)
        {
            DirectoryInfo tempLocation = null;

            try
            {
                Image manifestResult = null;
                string path;
                long size;

                var temporaryDirectory = _pathMapper.GetDirectory(DownloadsDirectoryPath);

                var fileName = string.Empty;

                var files = _fileSystemProvider.SearchFiles(temporaryDirectory.FullName, packageId + ".*");
                if (files.Length > 0)
                {
                    fileName = files[0];
                }

                var archiveFormat = GetArchiveFormat(fileName);

                string manifestType;

                if (archiveFormat != ArchiveFormat.None)
                {
                    // 1. Unpack Package.
                    tempLocation = Unpack(fileName, archiveFormat);

                    // 2. Read manifest.
                    var manifest = ReadManifest(packageId, tempLocation.FullName);

                    manifestType = manifest.Type;
                    var packageLocation = manifestType.Equals(NoteAcceptor) || manifestType.Equals(Printer)
                        ? tempLocation
                        : MovePackageToStorage(tempLocation.FullName, packageId);

                    manifestResult = manifest;
                    var fileInfo = packageLocation.GetFiles($"{packageId}.*", SearchOption.TopDirectoryOnly)
                        .FirstOrDefault(a => !a.Extension.ToLower(CultureInfo.CurrentCulture).Contains("manifest"));
                    path = fileInfo?.FullName;
                    size = fileInfo?.Length ?? 0;
                }
                else
                {
                    var fileInfo = new FileInfo(fileName);

                    manifestType = fileInfo.Extension.TrimStart('.');

                    path = fileInfo.FullName;
                    size = fileInfo.Length;
                }

                updateAction?.Invoke();

                var installer = _installerFactory.CreateNew(manifestType);

                if (!installer.Install(packageId))
                {
                    throw new CommandException("Installer failed");
                }

                if (tempLocation != null)
                {
                    _fileSystemProvider.DeleteFolder(tempLocation.FullName);
                }

                return (manifestResult, path, size, installer.DeviceChanged, installer.ExitAction);
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"Installation Error - Package {packageId} failed with message {exception.Message}",
                    exception);

                var packages = _pathMapper.GetDirectory(StorageDirectoryPath);

                var files = _fileSystemProvider.SearchFiles(packages.FullName, packageId + ".*");
                foreach (var file in files)
                {
                    _fileSystemProvider.DeleteFile(Path.Combine(packages.FullName, file));
                }

                if (tempLocation != null)
                {
                    _fileSystemProvider.DeleteFolder(tempLocation.FullName);
                }

                throw;
            }
        }

        /// <inheritdoc />
        public bool ValidateSoftwarePackage(string filePath)
        {
            if (GetArchiveFormat(filePath) == ArchiveFormat.None)
            {
                return true;
            }

            try
            {
                var packageId = Path.GetFileNameWithoutExtension(filePath);

                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                using (var zipFile = new ZipFile(stream))
                {
                    if (zipFile.Count != ArchiveFileCount)
                    {
                        Logger.Error($"Invalid file count {zipFile.Count} in the package archive: {filePath}");
                        return false;
                    }

                    var manifestId = zipFile.FindEntry($"{packageId}.{ManifestExtension}", true);
                    if (manifestId == -1)
                    {
                        Logger.Error($"Manifest file missing from package: {filePath}");
                    }

                    var manifestStream = zipFile.GetInputStream(manifestId);
                    var manifest = _manifestService.Read(() => manifestStream);

                    var isoFile = zipFile.GetEntry(manifest.File);
                    if (isoFile == null)
                    {
                        Logger.Error($"Referenced file ({manifest.File}) missing from package: {filePath}");
                        return false;
                    }

                    if (!ValidateManifestHash(manifest))
                    {
                        Logger.Error($"Failed to validate manifest hash: {filePath}");
                        return false;
                    }

                    if (!ValidateFileHash(zipFile, isoFile, manifest))
                    {
                        Logger.Error($"Failed to validate file hash: {filePath}");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to validate package file: {filePath}", e);
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public (long size, string name) BundleSoftwarePackage(string packageId, bool overwrite, string format = null)
        {
            var temporaryDirectory = _pathMapper.GetDirectory(DownloadsDirectoryPath);

            var fileName = Path.Combine(
                temporaryDirectory.FullName,
                $"{packageId}{format ?? $".{ArchiveFormat.Zip}"}");
            var archiveFormat = GetArchiveFormat(fileName);

            if (overwrite)
            {
                DeletePackageFile(packageId);
            }

            var unpackDirectory = _pathMapper.GetDirectory(StorageDirectoryPath);

            using (var stream = _fileSystemProvider.GetFileWriteStream(fileName))
            {
                _packageService.Pack(
                    archiveFormat,
                    unpackDirectory.FullName,
                    packageId,
                    stream);
            }

            var fileInfo = _fileSystemProvider.CreateFile(fileName);

            return (fileInfo.Length, fileName);
        }

        /// <inheritdoc />
        public (Image manifest, bool deviceChanged) UninstallSoftwarePackage(
            string packageId,
            Action<string[]> uninstalledAction = null)
        {
            var manifest = ReadManifest(packageId);

            var installer = _installerFactory.CreateNew(manifest.Type);

            if (installer.Uninstall(packageId))
            {
                var packages = _pathMapper.GetDirectory(StorageDirectoryPath);

                var files = _fileSystemProvider.SearchFiles(packages.FullName, packageId + ".*");

                uninstalledAction?.Invoke(files);

                foreach (var file in files)
                {
                    _fileSystemProvider.DeleteFile(Path.Combine(packages.FullName, file));
                }
            }
            else
            {
                throw new CommandException("Installer failed");
            }

            return (manifest, installer.DeviceChanged);
        }

        /// <inheritdoc />
        public Image ReadManifest(string packageId, string path = default(string))
        {
            var packages = path == default(string) ? _pathMapper.GetDirectory(StorageDirectoryPath) : new DirectoryInfo(path);

            var manifestFile =
                _fileSystemProvider.SearchFiles(packages.FullName, $"{packageId}.{ManifestExtension}").FirstOrDefault();
            if (string.IsNullOrEmpty(manifestFile))
            {
                throw new CommandException(
                    Localizer.For(CultureFor.Operator).FormatString(
                        ResourceKeys.UnableFindUniqueFileByExtensionErrorMessage,
                        ManifestExtension));
            }

            return _manifestService.Read(manifestFile);
        }

        /// <inheritdoc />
        public void DeleteSoftwarePackage(string packageId)
        {
            var temporaryDirectory = _pathMapper.GetDirectory(DownloadsDirectoryPath);

            var fileName = Path.Combine(temporaryDirectory.FullName, packageId);

            if (!fileName.EndsWith(".zip", StringComparison.CurrentCultureIgnoreCase))
            {
                var files = _fileSystemProvider.SearchFiles(temporaryDirectory.FullName, packageId + ".*");
                if (files.Length > 0)
                {
                    fileName = files[0];
                }
                else
                {
                    Logger.Error($"Unable to find package file to delete {fileName}");
                }
            }

            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                    Logger.Info($"Package file {fileName} deleted.");
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to delete package file {fileName} exception={e}");
                }
            }
            else
            {
                Logger.Error($"Unable to delete package file {fileName}");
            }
        }

        private ArchiveFormat GetArchiveFormat(string fileName)
        {
            var fileExt = Path.GetExtension(fileName);

            if (string.Equals(fileExt, $".{ArchiveFormat.Zip}", StringComparison.InvariantCultureIgnoreCase))
            {
                return ArchiveFormat.Zip;
            }

            if (string.Equals(fileExt, $".{ArchiveFormat.Tar}", StringComparison.InvariantCultureIgnoreCase))
            {
                return ArchiveFormat.Tar;
            }

            return ArchiveFormat.None;
        }

        private void DeletePackageFile(string packageId)
        {
            var fileName = GetFileName(packageId);

            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        private string GetFileName(string packageId)
        {
            var dir = _pathMapper.GetDirectory(DownloadsDirectoryPath);
            var fileName = Path.Combine(dir.FullName, packageId);

            if (!Path.HasExtension(fileName))
            {
                var files = _fileSystemProvider.SearchFiles(dir.FullName, packageId + ".*");
                if (files.Length > 0)
                {
                    fileName = files[0];
                }
            }

            return fileName;
        }

        private DirectoryInfo MovePackageToStorage(string temp, string packageId)
        {
            var packages = _pathMapper.GetDirectory(StorageDirectoryPath);

            var files = _fileSystemProvider.SearchFiles(temp, packageId + ".*");
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var target = Path.Combine(packages.FullName, fileInfo.Name);
                if (File.Exists(target))
                {
                    File.Delete(target);
                }

                fileInfo.MoveTo(target);
            }

            return packages;
        }

        private DirectoryInfo Unpack(string fileName, ArchiveFormat format)
        {
            var downloads = _pathMapper.GetDirectory(DownloadsDirectoryPath);

            var destination =
                _fileSystemProvider.CreateFolder(Path.Combine(downloads.FullName, TemporaryDirectoryPath));

            using (var stream = _fileSystemProvider.GetFileReadStream(fileName))
            {
                _packageService.Unpack(format, destination.FullName, stream);
            }

            return destination;
        }

        private static bool ValidateManifestHash(Image manifest)
        {
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                foreach (var line in manifest.Contents.Take(manifest.Contents.Length - 2))
                {
                    writer.WriteLine(line);
                }

                writer.Flush();

                stream.Seek(0, SeekOrigin.Begin);
                using (var hmac = new HMACSHA1(Secret))
                {
                    var computed = hmac.ComputeHash(stream);

                    var bytes = StringToByteArray(manifest.ManifestHash);

                    return computed.SequenceEqual(bytes);
                }
            }
        }

        private static bool ValidateFileHash(ZipFile zipFile, ZipEntry isoFile, Image manifest)
        {
            using (var sha = SHA1.Create())
            {
                var isoStream = zipFile.GetInputStream(isoFile);

                var encoded = Convert.ToBase64String(sha.ComputeHash(isoStream));

                return encoded.Equals(manifest.FileHash, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}
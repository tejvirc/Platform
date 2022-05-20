namespace Aristocrat.Monaco.Application.Authentication
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Cryptography;
    using Common.Storage;
    using Contracts.Authentication;
    using Contracts.Localization;
    using Hardware.Contracts;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using Monaco.Localization.Properties;
    using TaskExtensions = Common.TaskExtensions;

    /// <summary>
    ///     Implementation of <see cref="IAuthenticationService" />.
    /// </summary>
    [CLSCompliant(false)]
    public class AuthenticationService : IAuthenticationService, IDisposable
    {
        private const int BlockSize = 8192;
        private const int CrcTimeout = 5000;
        private const string ManifestExtension = "manifest";
        private static readonly ISet<ComponentType> ManifestComponents = new HashSet<ComponentType>
        {
            ComponentType.Module,
            ComponentType.Package
        };

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AutoResetEvent _waiter = new(true);
        private readonly IComponentRegistry _componentRegistry;
        private readonly IEventBus _eventBus;
        private readonly IFileSystemProvider _fileSystemProvider;

        private readonly ConcurrentDictionary<string, ComponentVerification> _verifications = new();

        private bool _disposed;

        /// <summary>
        ///     Constructor for <see cref="AuthenticationService" />
        /// </summary>
        /// <param name="componentRegistry">Reference to <see cref="IComponentRegistry" /> implementation</param>
        /// <param name="eventBus">Reference to <see cref="IEventBus" /> implementation</param>
        /// <param name="fileSystemProvider">Reference to <see cref="IFileSystemProvider"/></param>
        public AuthenticationService(
            IComponentRegistry componentRegistry,
            IEventBus eventBus,
            IFileSystemProvider fileSystemProvider)
        {
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _fileSystemProvider = fileSystemProvider ?? throw new ArgumentNullException(nameof(fileSystemProvider));
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IAuthenticationService) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public IEnumerable<AlgorithmType> SupportedAlgorithms =>
            Enum.GetValues(typeof(AlgorithmType)).OfType<AlgorithmType>();

        /// <inheritdoc />
        public IEnumerable<ComponentVerification> GetVerifications()
        {
            if (_verifications.Count == 0)
            {
                // Create our own version of the component registry.
                _componentRegistry.Components.ToList().ForEach(c => { AddComponentVerification(c); });
            }

            return _verifications.Values;
        }

        /// <inheritdoc />
        public ComponentVerification GetVerification(Component key)
        {
            return _verifications.TryGetValue(key.ComponentId, out var ver) ? ver : AddComponentVerification(key);
        }

        /// <inheritdoc />
        public void CalculateHash(
            Component component,
            ComponentVerification verification)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (verification == null)
            {
                throw new ArgumentNullException(nameof(verification));
            }

            var algorithm = verification.AlgorithmType.ToString().ToUpperInvariant();

            byte[] saltBytes = null;
            byte[] keyBytes = null;

            switch (verification.AlgorithmType)
            {
                case AlgorithmType.Md5:
                case AlgorithmType.Sha1:
                case AlgorithmType.Sha256:
                case AlgorithmType.Sha384:
                case AlgorithmType.Sha512:
                    if (verification.Salt != null)
                    {
                        saltBytes = verification.Salt;
                    }
                    break;
                case AlgorithmType.HmacSha1:
                case AlgorithmType.HmacSha256:
                case AlgorithmType.HmacSha512:
                    // For HmacSha1 the salt is the key (as defined in the protocol)
                    if (verification.Salt != null)
                    {
                        keyBytes = verification.Salt;
                    }
                    break;
                case AlgorithmType.Crc32:
                    switch (component.Path)
                    {
                        case Constants.PrinterPath:
                            var printer = ServiceManager.GetInstance().GetService<IDeviceRegistryService>()
                                .GetDevice<IPrinter>();
                            GetCrc(verification, printer);
                            return;
                        case Constants.NoteAcceptorPath:
                            var noteAcceptor = ServiceManager.GetInstance().GetService<IDeviceRegistryService>()
                                .GetDevice<INoteAcceptor>();
                            GetCrc(verification, noteAcceptor);
                            return;
                    }

                    throw new NotSupportedException(
                        string.Format(
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotSupportedAlgorithmTypeErrorMessageTemplate),
                            verification.AlgorithmType));
                case AlgorithmType.Crc16:
                    // We use the keyBytes for the seed
                    keyBytes = verification.Seed ?? Array.Empty<byte>();

                    break;
                default:
                    throw new NotSupportedException(
                        string.Format(
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotSupportedAlgorithmTypeErrorMessageTemplate),
                            verification.AlgorithmType));
            }

            Stream stream = new MemoryStream(Array.Empty<byte>());

            try
            {
                switch (component.FileSystemType)
                {
                    case FileSystemType.Directory:
                        stream = new DirectoryStream(new DirectoryInfo(component.Path));
                        break;
                    case FileSystemType.File:
                        stream = new FileStream(component.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        break;
                    case FileSystemType.Stream:
                        switch (component.Type)
                        {
                            case ComponentType.OS:
                                var osService = ServiceManager.GetInstance().GetService<IOSService>();
                                stream = new MemoryStream(osService.VirtualPartitions.GetOperatingSystemHash());
                                break;
                            case ComponentType.Hardware:
                                switch (component.Path)
                                {
                                    case Constants.PrinterPath:
                                        var printer = ServiceManager.GetInstance().GetService<IDeviceRegistryService>().GetDevice<IPrinter>();
                                        if (printer.Crc == 0)
                                        {
                                            printer.CalculateCrc(0).Wait(CrcTimeout);
                                        }

                                        stream = printer.Crc != 0 ? new MemoryStream(Encoding.UTF8.GetBytes(ConvertExtensions.ToHexString(printer.Crc))) : null;
                                        break;
                                    case Constants.NoteAcceptorPath:
                                        var noteAcceptor = ServiceManager.GetInstance().GetService<IDeviceRegistryService>().GetDevice<INoteAcceptor>();
                                        if (noteAcceptor.Crc == 0)
                                        {
                                            noteAcceptor.CalculateCrc(0).Wait(CrcTimeout);
                                        }

                                        stream = noteAcceptor.Crc != 0 ? new MemoryStream(Encoding.UTF8.GetBytes(ConvertExtensions.ToHexString(noteAcceptor.Crc))) : null;
                                        break;
                                    case Constants.FpgaPath:
                                    case Constants.BiosPath:
                                        var io = ServiceManager.GetInstance().GetService<IIO>();
                                        stream = new MemoryStream(io.GetFirmwareData(component.Path == Constants.FpgaPath ? FirmwareData.Fpga : FirmwareData.Bios));
                                        break;
                                    default:
                                        throw new Exception($"Can't create stream for hardware component {component.ComponentId}");
                                }

                                break;
                            default:
                                throw new Exception($"Can't create stream for component {component.ComponentId}");
                        }

                        break;
                }

                verification.Result = ComputeHash(
                    stream,
                    algorithm,
                    saltBytes,
                    keyBytes,
                    verification.StartOffset,
                    verification.EndOffset);

                verification.ResultTime = DateTime.UtcNow;
            }
            finally
            {
                stream?.Close();
            }
        }

        /// <inheritdoc />
        public byte[] ComputeHash(
            Stream stream,
            string algorithm,
            byte[] salt = null,
            byte[] key = null,
            long startOffset = 0,
            long endOffset = (long)AuthenticationServiceConstants.EndOfStream)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw new ArgumentNullException(nameof(algorithm));
            }

            ValidateStream(stream, startOffset, endOffset);

            if (startOffset == stream.Length)
            {
                startOffset = 0;
            }

            if (endOffset == (long)AuthenticationServiceConstants.EndOfStream || endOffset == 0)
            {
                endOffset = stream.Length;
            }

            // Use either the HMAC or HashAlgorithm factory to create the hash object
            using (var hash = CreateHash(algorithm, key))
            {
                if (ReferenceEquals(null, hash))
                {
                    throw new ArgumentException(
                        string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotValidAlgorithmErrorMessageTemplate), algorithm),
                        nameof(algorithm));
                }

                // Only apply the salt if it was provided
                if (salt != null)
                {
                    hash.TransformBlock(salt, 0, salt.Length, salt, 0);
                }

                ConfigureStream(stream, startOffset);

                var buffer = new byte[BlockSize];

                // Hash the data from startOffset to EOF or endOffset
                var remaining = (endOffset <= startOffset ? stream.Length : endOffset) - startOffset;
                var wrapped = false;

                while (remaining > 0)
                {
                    var bytesRead = stream.Read(buffer, 0, (int)Math.Min(remaining, BlockSize));
                    remaining -= bytesRead;

                    if (!wrapped && remaining == 0 && endOffset <= startOffset)
                    {
                        // In the event that the end offset is less than or equal to the start offset we need to go back to beginning and read to the end offset
                        stream.Seek(0, SeekOrigin.Begin);
                        remaining = endOffset;
                        wrapped = true;
                    }

                    hash.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                }

                // Do this even if nothing was hashed
                hash.TransformFinalBlock(buffer, 0, 0);

                return hash.Hash;
            }
        }

        /// <inheritdoc />
        public bool VerifyHash(
            byte[] hash,
            Stream stream,
            string algorithm,
            byte[] key = null,
            long offset = 0,
            long endOffset = (long)AuthenticationServiceConstants.EndOfStream)
        {
            if (hash == null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            if (algorithm == null)
            {
                return false;
            }

            var computed = ComputeHash(stream, algorithm, null, key, offset, endOffset);

            return computed.SequenceEqual(computed);
        }

        /// <inheritdoc />
        public Task GetComponentHashesAsync(
            AlgorithmType algorithmType,
            CancellationToken cancellationToken,
            byte[] seedOrSalt,
            IEnumerable<ComponentType> componentTypes)
        {
            return GetComponentHashesAsync(
                algorithmType,
                cancellationToken,
                seedOrSalt,
                _componentRegistry.Components.Where(c => componentTypes.Contains(c.Type)),
                0);
        }

        /// <inheritdoc />
        public Task GetComponentHashesAsync(
            AlgorithmType algorithmType,
            CancellationToken cancellationToken,
            byte[] seedOrSalt = null,
            string singleComponent = null,
            long offset = 0)
        {
            IEnumerable<Component> results;
            if (string.IsNullOrEmpty(singleComponent))
            {
                results = _componentRegistry.Components;
            }
            else
            {
                results = _componentRegistry.Components.Where(
                    component => component.ComponentId.Equals(singleComponent));
            }

            return GetComponentHashesAsync(algorithmType, cancellationToken, seedOrSalt, results, offset);
        }

        /// <inheritdoc />
        public int CalculateRomCrc32(int seed)
        {
            var result = Crc32.Compute((uint)seed, Encoding.UTF8.GetBytes(CalculateRomHash(AlgorithmType.Sha256)));

            Logger.Debug($"CalculateRomCrc32: Seed {seed} CRC 32 Int:{(int)result} Uint:{result} Hex:{result:X}");

            return (int)result;
        }

        /// <inheritdoc />
        public string CalculateRomHash(AlgorithmType algorithmType)
        {
            var osService = ServiceManager.GetInstance().GetService<IOSService>();
            var toHash = osService.VirtualPartitions.GetOperatingSystemHash().ToList();
            foreach (var component in _componentRegistry.Components
                .Where(x => ManifestComponents.Contains(x.Type) && !string.IsNullOrEmpty(x.Path)).OrderBy(x => x.ComponentId))
            {
                var manifest = _fileSystemProvider.SearchFiles(
                    Path.GetDirectoryName(component.Path),
                    $"{Path.GetFileNameWithoutExtension(component.Path)}.{ManifestExtension}").FirstOrDefault();
                if (manifest is null)
                {
                    Logger.Warn($"Missing a manifest file for {component.ComponentId} of type {component.Type}");
                    continue;
                }

                Logger.Debug($"Hashing the manifest for component {component.ComponentId} of type {component.Type}");

                toHash.AddRange(File.ReadAllBytes(manifest));
            }

            var hash = HashAlgorithm.Create(algorithmType.ToString().ToUpperInvariant())?.ComputeHash(toHash.ToArray());
            return GetHash(hash);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private static string GetHash(byte[] data)
        {
            var sBuilder = new StringBuilder();
            foreach (var toChar in data)
            {
                sBuilder.Append(toChar.ToString("x2"));
            }

            return sBuilder.ToString().ToUpper();
        }

        private static HashAlgorithm CreateHash(string algorithm, byte[] key)
        {
            if (algorithm.StartsWith(@"HMAC", StringComparison.OrdinalIgnoreCase))
            {
                var hash = HMAC.Create(algorithm);
                hash.Key = key ?? Encoding.UTF8.GetBytes(string.Empty);
                return hash;
            }

            if (algorithm.Equals(AlgorithmType.Crc16.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var crcSeed = (ushort)((key?.Length ?? 0) > 0 ? BitConverter.ToUInt16(key, 0) : 0);
                return new Crc16Ccitt(crcSeed);
            }


            return HashAlgorithm.Create(algorithm);
        }

        private void ConfigureStream(Stream stream, long startOffset)
        {
            // Position stream at the start offset position
            if (startOffset != 0)
            {
                stream.Seek(startOffset, SeekOrigin.Begin);
            }
        }

        private void ValidateStream(Stream stream, long startOffset, long endOffset)
        {
            // Parameter validation
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (startOffset < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startOffset),
                    startOffset,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StartOffsetLessZeroErrorMessage));
            }

            if (endOffset < (long)AuthenticationServiceConstants.EndOfStream)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(endOffset),
                    endOffset,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EndOffsetNegativeErrorMessage));
            }

            // Range and argument validation
            if (startOffset > 0 && startOffset > stream.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startOffset),
                    startOffset,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffsetMoreThanFileLengthErrorMessage));
            }

            if (startOffset > 0 && !stream.CanSeek)
            {
                throw new ArgumentException(
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StartOffsetNotSpecifiedForStreamErrorMessage),
                    nameof(startOffset));
            }

            if (endOffset > stream.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(endOffset),
                    endOffset,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffsetMoreThanFileLengthErrorMessage));
            }
        }

        private Task GetComponentHashesAsync(
            AlgorithmType algorithmType,
            CancellationToken cancellationToken,
            byte[] seedOrSalt,
            IEnumerable<Component> components,
            long offset)
        {
            // Limited to 75% of the available cores (rounded down)
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = TaskExtensions.MaxDegreeOfParallelism()
            };

            return new Task(
                () =>
                {
                    // wait for any in progress calculations to finish before starting more
                    _waiter.WaitOne();

                    var complete = true;
                    var result = new ParallelLoopResult();
                    try
                    {
                        // Asynchronously start each component hash
                        result = Parallel.ForEach(
                            components,
                            options,
                            component =>
                            {
                                ComponentVerification verification = null;
                                try
                                {
                                    verification = GetVerification(component);
                                    verification.AlgorithmType = algorithmType;
                                    verification.StartOffset = 0;
                                    verification.EndOffset = (long)AuthenticationServiceConstants.EndOfStream;
                                    verification.Salt = seedOrSalt;
                                    verification.Seed = seedOrSalt;
                                    verification.StartOffset = offset;
                                    verification.ClearResults();

                                    Logger.Debug($"Hash started for {component.ComponentId}");
                                    CalculateHash(component, verification);

                                    Logger.Debug($" Hash complete for {component.ComponentId}");
                                    _eventBus.Publish(new ComponentHashCompleteEvent(verification, cancellationToken));
                                }
                                catch (Exception ex)
                                {
                                    if (verification is null)
                                    {
                                        verification = new ComponentVerification
                                        {
                                            ComponentId = component.ComponentId,
                                            AlgorithmType = algorithmType,
                                            StartOffset = 0,
                                            EndOffset = (long)AuthenticationServiceConstants.EndOfStream,
                                            Salt = seedOrSalt,
                                            Seed = seedOrSalt
                                        };
                                        verification.StartOffset = offset;
                                        verification.ClearResults();
                                    }

                                    Logger.Error($"Exception calculating component hash for {component.ComponentId} : {ex.Message}");
                                    _eventBus.Publish(new ComponentHashErrorEvent(verification, cancellationToken, ex.Message));
                                    complete = false;
                                }
                            });
                    }
                    catch (Exception e)
                    {
                        Logger.Debug($"Parallel.ForEach exception: {e.Message}");
                    }
                    finally
                    {
                        _eventBus.Publish(new AllComponentsHashCompleteEvent(!complete || !result.IsCompleted, cancellationToken));
                        _waiter.Set();
                    }
                });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _verifications.Clear();
                _waiter.Close();
            }

            _disposed = true;
        }

        private static void GetCrc(ComponentVerification verification, IDeviceAdapter device)
        {
            var seed = verification.Seed.Length > 0 ? BitConverter.ToInt32(verification.Seed, 0) : 0;
            var result = device.CalculateCrc(seed).Result;
            verification.Result = BitConverter.GetBytes(result);
            verification.ResultTime = DateTime.UtcNow;
        }

        private ComponentVerification AddComponentVerification(Component component)
        {
            var compVer = new ComponentVerification
            {
                ComponentId = component.ComponentId
            };

            compVer.ClearResults();
            _verifications.AddOrUpdate(component.ComponentId, compVer, (_, _) => compVer);
            return compVer;
        }
    }
}
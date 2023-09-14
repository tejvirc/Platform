namespace Aristocrat.Monaco.Application.Authentication
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Authentication;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Contracts.Authentication;
    using Contracts.Localization;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.VHD;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Monaco.Localization.Properties;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using Signing;
    using Signing.Model;

    /// <summary>
    ///     Provides a mechanism to monitor events and trigger Live Authentication process.
    ///     This service is disabled by default.
    /// </summary>
    public class LiveAuthenticationManager : ILiveAuthenticationManager, IDisposable
    {
        private const string ManifestExtension = "manifest";
        private const string GameType = "game";
        private const string PlatformPrefix = @"ATI_platform";

        private static readonly Guid DisableGuid = ApplicationConstants.LiveAuthenticationDisableKey;
        // ReSharper disable once PossibleNullReferenceException
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string[] ExtensionExclusions =
            { ".mercury", ".png", ".bk2", ".gsaManifest", ".dat", ".exp", ".pdb", ".manifest", ".xlf", ".ttf",
              ".iobj", ".ipdb", ".pak", ".ogg", ".jpg", ".jpeg", ".xaml", ".db", ".xldf" };

        private static readonly string[] FolderExclusions = { @"downloads\temp" };

        private readonly ISystemDisableManager _disableManager;
        private readonly IEventBus _eventBus;
        private readonly IPathMapper _pathMapper;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IAudio _audioService;
        private readonly object _cancellationLock = new object();
        private readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

        private static DsaKeyParameters _systemKey;
        private static DsaKeyParameters _gameKey;
#if !(RETAIL)
        private static DsaKeyParameters _developmentKey;
#endif

        private CancellationTokenSource _cancellationTokenSource;

        private bool _enabled;

        private bool _disposed;
        
        public LiveAuthenticationManager()
         : this(ServiceManager.GetInstance().GetService<IEventBus>(),
             ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
             ServiceManager.GetInstance().GetService<IPropertiesManager>(),
             ServiceManager.GetInstance().GetService<IPathMapper>(),
             ServiceManager.GetInstance().TryGetService<IAudio>())
        {
        }

        public LiveAuthenticationManager(
            IEventBus eventBus,
            ISystemDisableManager disableManager,
            IPropertiesManager propertiesManager,
            IPathMapper pathMapper,
            IAudio audio)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _audioService = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ILiveAuthenticationManager) };

        /// <inheritdoc />
        public void Initialize()
        {
            var systemKeyFile = _propertiesManager.GetValue(KernelConstants.SystemKey, string.Empty);
            if (!string.IsNullOrEmpty(systemKeyFile))
            {
                using (var reader = File.OpenText(systemKeyFile))
                {
                    _systemKey = (DsaKeyParameters)new PemReader(reader).ReadObject();
                }
            }
#if !(RETAIL)
            else
            {
                _systemKey = GetDevelopmentKey();
            }
#endif

            var gameKeyFile = _propertiesManager.GetValue(KernelConstants.GameKey, string.Empty);
            if (!string.IsNullOrEmpty(gameKeyFile))
            {
                using (var reader = File.OpenText(gameKeyFile))
                {
                    _gameKey = (DsaKeyParameters)new PemReader(reader).ReadObject();
                }
            }
#if !(RETAIL)
            else
            {
                _gameKey = GetDevelopmentKey();
            }
#endif

#if (RETAIL)
            var manifest = GetPlatformManifest(_pathMapper.GetDirectory(ApplicationConstants.ManifestPath).FullName);

            AddAuthenticatedPath(Directory.GetCurrentDirectory(), manifest);
#endif

            _eventBus.Subscribe<DiskMountedEvent>(this, Handle);
            _eventBus.Subscribe<DiskUnmountedEvent>(this, Handle);

            bool runSignatureVerificationAfterReboot = (bool)_propertiesManager.GetProperty(
                ApplicationConstants.RunSignatureVerificationAfterReboot,
                false);

            if (runSignatureVerificationAfterReboot)
            {
                _eventBus.Subscribe<PlatformBootedEvent>(this, Handle);
            }

            Enabled = true;
        }

        public bool Enabled
        {
            get => _enabled;

            set
            {
                if (_enabled == value)
                {
                    return;
                }

                _enabled = value;
                if (value)
                {
                    OnEnabled();
                }
                else
                {
                    OnDisabled();
                }
            }
        }

        private event EventHandler<string> LiveAuthenticationFailedEvent;
        private event EventHandler LiveAuthenticationCanceledEvent;
        private event EventHandler LiveAuthenticationCompleteEvent;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            lock (_cancellationLock)
            {
                if (disposing)
                {
                    Enabled = false;

                    CancelTask();
                }

                _cancellationTokenSource = null;
            }

            _disposed = true;
        }

        private static string GetHashFromPath(string path)
        {
            string result = null;

            var directory = new DirectoryInfo(path);

            var files = directory.EnumerateFiles(@"*", SearchOption.AllDirectories)
                .Where(f => ExtensionExclusions.All(e => !e.Equals(f.Extension, StringComparison.InvariantCultureIgnoreCase)))
                .OrderBy(f => f.FullName)
                .ToList();

            if (files.Count > 0)
            {
                using (var stream = new DirectoryStream(files))
                using (var sha = new SHA1CryptoServiceProvider())
                {
                    result = Convert.ToBase64String(sha.ComputeHash(stream));
                }
            }

            return result;
        }

        private void Handle(DiskMountedEvent evt)
        {
            if (FolderExclusions.Any(exclusion => evt.PhysicalPath.Contains(exclusion)))
            {
                return;
            }

            var manifest = Path.ChangeExtension(evt.VirtualDisk, ManifestExtension);

            AddAuthenticatedPath(evt.PhysicalPath, manifest);
        }

        private void Handle(DiskUnmountedEvent evt)
        {
            _cache.TryRemove(evt.PhysicalPath, out _);

            Logger.Debug($"Removed path {evt.PhysicalPath}");
        }

        private void OnEnabled()
        {
            _eventBus.Subscribe<SystemDisableAddedEvent>(this, Handle);
            _eventBus.Subscribe<SystemDisableRemovedEvent>(this, Handle);

            LiveAuthenticationFailedEvent += HandleLiveAuthenticationFailedEvent;
            LiveAuthenticationCanceledEvent += HandleLiveAuthenticationCanceledEvent;
            LiveAuthenticationCompleteEvent += HandleLiveAuthenticationCompleteEvent;

            // If there are any other lockups active, then we should flag for verification now
            if (_disableManager.DisableImmediately)
            {
                _disableManager.Disable(DisableGuid, SystemDisablePriority.Immediate, () => string.Empty);
            }
        }

        private void OnDisabled()
        {
            _eventBus.Unsubscribe<SystemDisableAddedEvent>(this);
            _eventBus.Unsubscribe<SystemDisableRemovedEvent>(this);

            LiveAuthenticationFailedEvent -= HandleLiveAuthenticationFailedEvent;
            LiveAuthenticationCanceledEvent -= HandleLiveAuthenticationCanceledEvent;
            LiveAuthenticationCompleteEvent -= HandleLiveAuthenticationCompleteEvent;

            CancelTask();
        }

        private void Handle(PlatformBootedEvent evt)
        {
            _disableManager.Disable(
                DisableGuid,
                SystemDisablePriority.Immediate,
                () => Localizer.ForLockup().GetString(ResourceKeys.VerifyingSignaturesText));

            VerifySignatures();

            _eventBus.Unsubscribe<PlatformBootedEvent>(this);
        }

        private void Handle(SystemDisableAddedEvent evt)
        {
            // A system lockup was added, and it was not us, so we flag for verification
            if (evt.DisableId != DisableGuid)
            {
                lock (_cancellationLock)
                {
                    // Cancel the current verification task if there is one running
                    if (_cancellationTokenSource != null)
                    {
                        CancelTask();
                        _disableManager.Disable(DisableGuid, SystemDisablePriority.Immediate, () => string.Empty);
                    }
                    // Set the verification flag when the disable event is a 'Immediate' type and NOT caused by the operator menu
                    else if (evt.Priority == SystemDisablePriority.Immediate
                             && evt.DisableId != ApplicationConstants.OperatorMenuLauncherDisableGuid)
                    {
                        _disableManager.Disable(DisableGuid, SystemDisablePriority.Immediate, () => string.Empty);
                    }
                }
            }
        }

        private void Handle(SystemDisableRemovedEvent evt)
        {
            // Check if we are the only system disable that's left
            var disableManagerKeys = _disableManager.CurrentImmediateDisableKeys.ToList();
            if (disableManagerKeys.Count == 1 && disableManagerKeys.Contains(DisableGuid))
            {
                _disableManager.Disable(DisableGuid, SystemDisablePriority.Immediate,
                    () => Localizer.ForLockup().GetString(ResourceKeys.VerifyingSignaturesText));

                VerifySignatures();
            }
        }

        private void VerifySignatures()
        {
            lock (_cancellationLock)
            {
                // Make sure the previous verification task is canceled in case it's still running
                CancelTask();

                // Start verifying
                _cancellationTokenSource = new CancellationTokenSource();
                Task.Run(
                    () => VerifySignaturesAsync(_cancellationTokenSource.Token),
                    _cancellationTokenSource.Token).FireAndForget(ex => Logger.Error(ex.Message));
            }
        }

        private void HandleLiveAuthenticationFailedEvent(object sender, string message)
        {
            Logger.Fatal("Live Authentication failed - " + message);

            // Cancel the current verification task
            CancelTask();

            var displayMessage = Localizer.ForLockup()
                .FormatString(ResourceKeys.VerificationFailedText, message);

            // Update the disable text with the error message
            _disableManager.Disable(DisableGuid, SystemDisablePriority.Immediate,
                () => displayMessage);

            PlayErrorSound();

            _eventBus.Publish(new LiveAuthenticationFailedEvent(displayMessage));
        }

        private void HandleLiveAuthenticationCanceledEvent(object sender, EventArgs e)
        {
            Logger.Info("Live Authentication canceled");

            _disableManager.Enable(DisableGuid);
        }

        private void HandleLiveAuthenticationCompleteEvent(object sender, EventArgs e)
        {
            Logger.Info("Live Authentication complete");

            // Must dispose the Cancellation Token once the task is done
            CancelTask();

            _disableManager.Enable(DisableGuid);
        }

        private void AddAuthenticatedPath(string physicalPath, string manifest)
        {
            if (manifest == null || !File.Exists(manifest))
            {
#if !(RETAIL)
                Logger.Debug($"No manifest found for {physicalPath}. Verification will be skipped");
#else
                _cache.AddOrUpdate(physicalPath, (string)null, (key, value) => null);

                HandleLiveAuthenticationFailedEvent(this, Localizer.ForLockup().GetString(ResourceKeys.ManifestMissing));
#endif
                return;
            }

            Image image = null;

            try
            {
                image = ImageManifest.Read(new FileInfo(manifest), GetPublicKey, false);

#if !(RETAIL)
                if (string.IsNullOrEmpty(image?.AssemblyHash))
                {
                    Logger.Debug($"No assembly hash in {manifest}. Verification will be skipped");
                    return;
                }
#else
                if (image == null || string.IsNullOrEmpty(image.AssemblyHash))
                {
                    HandleLiveAuthenticationFailedEvent(this, Localizer.ForLockup().GetString(ResourceKeys.ManifestVerificationFailed));

                    Logger.Error($"Assembly hash is not present: {manifest}");
                }
#endif
            }
            catch (ManifestException e)
            {
                HandleLiveAuthenticationFailedEvent(this, e.Message);
            }
            catch (Exception e)
            {
                HandleLiveAuthenticationFailedEvent(this,
                    Localizer.ForLockup().GetString(ResourceKeys.ManifestVerificationFailed));

                Logger.Error("Failed to read manifest", e);
            }

            _cache.AddOrUpdate(physicalPath, image?.AssemblyHash, (_, _) => image?.AssemblyHash);

            Logger.Debug($"Added path {physicalPath}");
        }

        private void CancelTask()
        {
            lock (_cancellationLock)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }

        private void VerifySignaturesAsync(CancellationToken token)
        {
            var stopwatch = new Stopwatch();

            try
            {
                Logger.Info("Live Authentication started...");

                stopwatch.Start();

                // Limit the number of concurrent tasks enabled by Parallel operation to avoid the Platform becoming completely unresponsive.
                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = Common.TaskExtensions.MaxDegreeOfParallelism()
                };

                Parallel.ForEach(
                    _cache,
                    parallelOptions,
                    (item, _) =>
                    {
                        Logger.Info($"Validating: {item.Key} - {item.Value}");

                        if (string.IsNullOrEmpty(item.Value) || !item.Value.Equals(GetHashFromPath(item.Key)))
                        {
                            throw new AuthenticationException($"Signature verification failed for path: {item.Key}");
                        }
                    });

                stopwatch.Stop();

                Logger.Info($"Verification complete: {stopwatch.Elapsed}");

                LiveAuthenticationCompleteEvent?.Invoke(this, EventArgs.Empty);
            }
            catch (OperationCanceledException)
            {
                LiveAuthenticationCanceledEvent?.Invoke(this, EventArgs.Empty);
            }
            catch (AggregateException e)
            {
                LiveAuthenticationFailedEvent?.Invoke(this, e.Flatten().InnerExceptions.FirstOrDefault()?.Message);
            }
            catch (Exception e)
            {
                LiveAuthenticationFailedEvent?.Invoke(this, e.Message);
            }
        }

        private static DsaKeyParameters GetPublicKey(string type)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException(nameof(type));

            return type.Equals(GameType, StringComparison.InvariantCultureIgnoreCase) ? _gameKey : _systemKey;
        }

        private static string GetPlatformManifest(string path)
        {
            var directory = new DirectoryInfo(path);

            var files = directory.GetFiles($@"{PlatformPrefix}*.{ManifestExtension}");

            return files.OrderByDescending(f => f.Name).FirstOrDefault()?.FullName;
        }

#if !(RETAIL)
        private static DsaKeyParameters GetDevelopmentKey()
        {
            if (_developmentKey == null)
            {
                const string key = "dev_pub.pem";

                using (var reader = new StreamReader(key))
                {
                    var devKeyText = reader.ReadToEnd();
                    using (var dummyReader = new StringReader(devKeyText))
                    {
                        _developmentKey = (DsaKeyParameters)new PemReader(dummyReader).ReadObject();
                    }
                }
            }

            return _developmentKey;
        }
#endif
        
        /// <summary>
        /// Plays the sound defined in the Application Config for LiveAuthentication check failures.
        /// </summary>
        private void PlayErrorSound()
        {
            var alertVolume = _propertiesManager.GetValue(ApplicationConstants.AlertVolumeKey, _audioService.DefaultAlertVolume);
            _audioService.PlayAlert(SoundName.LiveAuthenticationFailed, alertVolume);
        }
    }
}
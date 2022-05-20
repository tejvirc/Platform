namespace Aristocrat.Monaco.Application.Drm
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Common.Ant;
    using Contracts;
    using Contracts.Drm;
    using Contracts.Localization;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Monaco.Localization.Properties;
    using Newtonsoft.Json;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using Signing;

    public class DigitalRights : IDigitalRights, IService
    {
        private const string LicenseFile = @"license.info";
        private const string LicenseFilePath = "/Manifests";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TimeSpan StatusInterval = TimeSpan.FromMinutes(5);

        private readonly IEventBus _bus;
        private readonly ISystemDisableManager _disableManager;
        private readonly IPathMapper _pathMapper;
        private readonly IPropertiesManager _properties;

        private IProtectionModule _protectionModule;
        private Timer _connectedTimer;

        private bool _disposed;

        public DigitalRights()
            : this(
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IPathMapper>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public DigitalRights(
            IPropertiesManager properties,
            ISystemDisableManager disableManager,
            IPathMapper pathMapper,
            IEventBus bus)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        private bool InternalDisabled => Disabled ||
                                         _disableManager.CurrentDisableKeys.Contains(ApplicationConstants.SmartCardExpiredDisableKey) ||
                                         _disableManager.CurrentDisableKeys.Contains(ApplicationConstants.LicenseErrorDisableKey);

        public bool Disabled => _disableManager.CurrentDisableKeys.Contains(ApplicationConstants.SmartCardRemovedDisableKey) ||
                                _disableManager.CurrentDisableKeys.Contains(ApplicationConstants.SmartCardNotPresentDisableKey);

        public ILicense License { get; private set; }

        public TimeSpan TimeRemaining => GetCounterValue(
            Counter.TimeRemaining,
            v => v != int.MaxValue ? TimeSpan.FromSeconds(v) : Timeout.InfiniteTimeSpan,
            Timeout.InfiniteTimeSpan);

        public int LicenseCount => GetCounterValue(Counter.LicenseCount, v => v, int.MaxValue);

        public string JurisdictionId => _protectionModule?.Tokens?.FirstOrDefault()?.Data?.JurisdictionId ??
                                        _properties.GetValue(ApplicationConstants.JurisdictionId, "") ?? // backup source is command line
                                        string.Empty;

        public bool IsDeveloper => _protectionModule?.IsDeveloper ?? true;

        public bool IsLicensed(FileInfo media)
        {
            var fileName = Path.GetFileNameWithoutExtension(media.Name);

            Logger.Debug($"Validating license for: {fileName}");

#if (!RETAIL)
            if (!License?.Licenses.Any() ?? true)
            {
                return true;
            }
#endif

            // Currently, if a developer card is present all game licenses will be valid
            return (_protectionModule?.IsDeveloper ?? false) || License.Licenses.Select(l => new Ant(l)).Any(l => l.IsMatch(fileName));
        }

        public bool IsAuthorized(string jurisdictionId)
        {
            Logger.Debug($"Validating jurisdiction Id for: {jurisdictionId}");

            return string.IsNullOrEmpty(JurisdictionId) ||
                   JurisdictionId.Equals(jurisdictionId, StringComparison.InvariantCultureIgnoreCase);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Initialize()
        {
            License = GetLicense(_pathMapper.GetDirectory(LicenseFilePath));

            var smartCardKeyFile = _properties.GetValue(KernelConstants.SmartCardKey, string.Empty);

#if !(RETAIL)
            if (string.IsNullOrEmpty(smartCardKeyFile))
            {
                Logger.Debug("No key file defined.  Protection module will be disabled");

                return;
            }
#endif

            _protectionModule = new SmartCardModule(smartCardKeyFile);

            try
            {
                _protectionModule.Initialize().Wait();

                if (_protectionModule.Tokens.All(t => t.Name != License.Id))
                {
                    Logger.Error($"Mismatch between the license file and the available tokens: {License.Id}");

                    var message = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LicenseFileValidationError);

                    HandleDigitalRightsError(new LicenseErrorEvent(message), () => message, ApplicationConstants.LicenseErrorDisableKey);

                    return;
                }

                _connectedTimer = new Timer(OnStatus, null, StatusInterval, Timeout.InfiniteTimeSpan);

                Logger.Info("Initialized the module");
            }
            catch (Exception e)
            {
                var message = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SmartCardMissing);
                HandleDigitalRightsError(new SoftwareProtectionModuleErrorEvent(message), () => message, ApplicationConstants.SmartCardNotPresentDisableKey);

                Logger.Error("Failed to initialize the module", e);
            }
        }

        public string Name => typeof(IDigitalRights).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IDigitalRights) };

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // ReSharper disable once UseNullPropagation
                if (_protectionModule != null)
                {
                    _protectionModule.Dispose();
                }

                if (_connectedTimer != null)
                {
                    var timer = _connectedTimer;
                    _connectedTimer = null;
                    timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

                    using (var handle = new ManualResetEvent(false))
                    {
                        timer.Dispose(handle);
                        if (!handle.WaitOne(TimeSpan.FromSeconds(5)))
                        {
                            Logger.Error("Timed out waiting for _connectedTimer.Dispose");
                        }
                    }
                }
            }

            _connectedTimer = null;
            _protectionModule = null;

            _disposed = true;
        }

#if !(RETAIL)
        private static DsaKeyParameters GetDevelopmentKey()
        {
            const string key = "dev_pub.pem";

            using (var reader = new StreamReader(key))
            {
                var devKeyText = reader.ReadToEnd();
                using (var dummyReader = new StringReader(devKeyText))
                {
                    return (DsaKeyParameters)new PemReader(dummyReader).ReadObject();
                }
            }
        }
#endif

        private ILicense GetLicense(FileSystemInfo directory)
        {
            var file = new FileInfo(Path.Combine(directory.FullName, LicenseFile));
            if (!file.Exists)
            {
#if RETAIL
                var message = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LicenseFileMissing);
                HandleDigitalRightsError(
                    new LicenseErrorEvent(message),
                    () => message,
                    ApplicationConstants.LicenseErrorDisableKey);
#endif

                return LicenseInfo.Invalid;
            }

            try
            {
#if RETAIL
                DsaKeyParameters key = null;
#else
                DsaKeyParameters key;
#endif

                var systemKeyFile = _properties.GetValue(KernelConstants.SystemKey, string.Empty);
                if (!string.IsNullOrEmpty(systemKeyFile))
                {
                    using (var reader = File.OpenText(systemKeyFile))
                    {
                        key = (DsaKeyParameters)new PemReader(reader).ReadObject();
                    }
                }
#if !(RETAIL)
                else
                {
                    key = GetDevelopmentKey();
                }
#endif
                SignedManifest.Validate(file, key);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to authenticate license file", ex);

                var message = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LicenseFileValidationError);
                HandleDigitalRightsError(new LicenseErrorEvent(message), () => message, ApplicationConstants.LicenseErrorDisableKey);

                return LicenseInfo.Invalid;
            }

            try
            {
                var contents = File.ReadAllLines(file.FullName);

                var data = string.Join(string.Empty, contents.Take(contents.Length - 2));

                return JsonConvert.DeserializeObject<LicenseInfo>(data);
            }
            catch (Exception ex)
            {
                var message = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LicenseFileParsingError);
                HandleDigitalRightsError(new LicenseErrorEvent(message), () => message, ApplicationConstants.LicenseErrorDisableKey);

                Logger.Error("Failed to read the license file", ex);
            }

            return LicenseInfo.Invalid;
        }

        private void OnStatus(object state)
        {
            try
            {
                var token = _protectionModule.Tokens.First();

                if ((_protectionModule.IsDeveloper || (token.Counters.ContainsKey(Counter.TimeRemaining) && TimeRemaining != Timeout.InfiniteTimeSpan)) &&
                    !_protectionModule.DecrementCounter(token, Counter.TimeRemaining, (int)StatusInterval.TotalSeconds))
                {
                    if (!InternalDisabled)
                    {
                        Logger.Error("Failed to decrement the license counter");

                        var message = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SmartCardExpired);
                        HandleDigitalRightsError(new SoftwareProtectionModuleErrorEvent(message), () => message, ApplicationConstants.SmartCardExpiredDisableKey);
                    }
                }
                else if (!_protectionModule.IsConnected())
                {
                    if (!InternalDisabled)
                    {
                        Logger.Error("Failed to get the module state");

                        var message = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SmartCardRemoved);
                        HandleDigitalRightsError(new SoftwareProtectionModuleDisconnectedEvent(message), () => message, ApplicationConstants.SmartCardRemovedDisableKey);
                    }
                }
                else if (InternalDisabled)
                {
                    Logger.Info("Module validation passed. Re-enabling the system");

                    _disableManager.Enable(ApplicationConstants.SmartCardRemovedDisableKey);
                    _disableManager.Enable(ApplicationConstants.SmartCardExpiredDisableKey);
                    _disableManager.Enable(ApplicationConstants.SmartCardNotPresentDisableKey);
                    _disableManager.Enable(ApplicationConstants.LicenseErrorDisableKey);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to get the module state", e);
                var message = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SmartCardRemoved);
                HandleDigitalRightsError(new SoftwareProtectionModuleErrorEvent(message), () => message, ApplicationConstants.SmartCardRemovedDisableKey);
            }

            _connectedTimer?.Change(StatusInterval, Timeout.InfiniteTimeSpan);
        }

        private void HandleDigitalRightsError<T>(T @event, Func<string> message, Guid disableKey)
            where T : IEvent
        {
            _bus.Publish(@event);

            _disableManager.Disable(disableKey, SystemDisablePriority.Immediate, message);
        }

        private T GetCounterValue<T>(Counter counter, Func<int, T> converter, T defaultValue)
        {
            if (_protectionModule == null)
            {
                return defaultValue;
            }

            try
            {
                var token = _protectionModule.Tokens.First();

                return token.Counters.TryGetValue(counter, out var value)
                    ? converter(Convert.ToInt32(value))
                    : defaultValue;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to get the counter value for {counter}", e);

                return defaultValue;
            }
        }

        private class LicenseInfo : ILicense
        {
            public static readonly ILicense Invalid = new LicenseInfo();

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Id { get; set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public Version Version { get; set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public Configuration Configuration { get; set; }

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public IEnumerable<string> Licenses { get; set; } = new List<string>();
        }
    }
}
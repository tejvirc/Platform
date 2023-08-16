namespace Aristocrat.Monaco.Application.EKey
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Hardware.Contracts.Cabinet;
    using Common;
    using Contracts;
    using Contracts.EKey;
    using Contracts.Localization;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;
    using SmartCard;

    /// <summary>
    ///     Implements <see cref="IEKey"/> interface.
    /// </summary>
    public sealed class EKeyService : BaseRunnable, IEKey, IService
    {
        private const int MonitorIntervalMs = 5000;

        private const string SmartCardReaderDescription = @"Microsoft Usbccid Smartcard Reader (WUDF)";

        private const string EKeyVolumeLabel = "EKEYDATA";

        private static readonly IReadOnlyList<string> OnBoardCardReaderNames = new List<string>
        {
            "ATA Mk7iICC 0",
            "MK7 Smart Card"
        };

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _disableManager;

        private readonly List<EKeyProgram> _verificationPrograms;

        private ManualResetEvent _readersConnected = new(false);

        private CancellationTokenSource _shutdown = new();

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EKeyService" /> class.
        /// </summary>
        public EKeyService()
            : this(
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EKeyService" /> class.
        /// </summary>
        /// <param name="properties">A <see cref="IEventBus"/> instance.</param>
        /// <param name="eventBus">A <see cref="IEventBus"/> instance.</param>
        /// <param name="disableManager">A <see cref="ISystemDisableManager"/></param>
        public EKeyService(IPropertiesManager properties, IEventBus eventBus, ISystemDisableManager disableManager)
        {
            _properties = properties;
            _eventBus = eventBus;
            _disableManager = disableManager;

            _verificationPrograms = new List<EKeyProgram> { new EKeyProdProgram(), new EKeyDevProgram() };
        }

        /// <inheritdoc />
        ~EKeyService()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IEKey) };

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            SubscribeToEvents();
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            try
            {
                while (RunState == RunnableState.Running && !_shutdown.IsCancellationRequested)
                {
                    Monitor(_shutdown.Token).GetAwaiter().GetResult();
                }
            }
            catch (ThreadInterruptedException)
            {
                // Do nothing we are shutting down
            }

            if (!_shutdown.IsCancellationRequested)
            {
                _shutdown.Cancel(true);
            }
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            _shutdown.Cancel(true);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _readersConnected?.Dispose();
                _shutdown?.Cancel(true);
                _shutdown?.Dispose();
            }

            _readersConnected = null;
            _shutdown = null;
            _disposed = true;
            base.Dispose(disposing);
        }

        private async Task Monitor(CancellationToken token)
        {
            var readers = new Dictionary<string, (string Reader, EKeyState State)>();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(MonitorIntervalMs, token);
                    await UpdateReaders(readers);
                    await Unverify(readers);
                    await Verify(readers);

                    // Don't continue to loop if no readers connected
                    if (readers.Any())
                    {
                        continue;
                    }

                    _readersConnected.Reset();
                    await _readersConnected.AsTask(token);
                }
            }
            catch (AggregateException ex) when (TaskCancelled(ex))
            {
                // Do nothing
            }
            catch (ThreadInterruptedException)
            {
                // Do nothing
            }
            catch (TaskCanceledException)
            {
                // Do nothing
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }
            catch (SmartCardException ex)
            {
                Logger.Error($"Error monitoring smart card readers, {ex.Message}", ex);
            }
            catch (AggregateException ex) when (SmartCardError(ex))
            {
                Logger.Error("Error monitoring smart card readers", ex);
            }
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<DeviceConnectedEvent>(this, Handle);
            _eventBus.Subscribe<DebugEKeyEvent>(this, Handle);
        }

        private void Handle(DeviceConnectedEvent evt)
        {
            if (string.Compare(
                evt.Description,
                SmartCardReaderDescription,
                StringComparison.OrdinalIgnoreCase) != 0)
            {
                return;
            }

            _readersConnected.Set();
        }

        private void Handle(DebugEKeyEvent evt)
        {
            SetEKeyVerified(evt.IsVerified, evt.Drive);
        }

        private void SetEKeyVerified(bool verified, string drive)
        {
            _properties.SetProperty(ApplicationConstants.EKeyVerified, verified);
            _properties.SetProperty(ApplicationConstants.EKeyDrive, drive);

            if (verified)
            {
                if (_disableManager.CurrentDisableKeys.Contains(ApplicationConstants.EKeyVerifiedDisableKey))
                {
                    return;
                }

                _disableManager.Disable(
                    ApplicationConstants.EKeyVerifiedDisableKey,
                    SystemDisablePriority.Normal,
                    () => Localizer.ForLockup().GetString(ResourceKeys.EKeyDetected));
            }
            else if (_disableManager.CurrentDisableKeys.Contains(ApplicationConstants.EKeyVerifiedDisableKey))
            {
                _disableManager.Enable(ApplicationConstants.EKeyVerifiedDisableKey);
            }
        }

        private static async Task UpdateReaders(IDictionary<string, (string Reader, EKeyState State)> readers)
        {
            try
            {
                var readerInfos = await ReaderInformation.FindAll();
                var readerNames = readerInfos
                    .Select(x => x.ReaderName)
                    .Except(OnBoardCardReaderNames, StringComparer.InvariantCultureIgnoreCase)
                    .ToList();

                var removedReaders = readers.Keys.Except(readerNames).ToArray();

                foreach (var name in removedReaders)
                {
                    readers.Remove(name);
                }

                var addedReaders = readerNames.Except(readers.Keys).ToArray();

                foreach (var name in addedReaders)
                {
                    readers.Add(name, (name, EKeyState.Disconnected));
                }
            }
            catch (SmartCardException ex)
            {
                Logger.Error($"Unable to update readers, {ex.Message}", ex);
            }
            catch (AggregateException ex) when (SmartCardError(ex))
            {
                Logger.Error("Unable to update readers", ex);
            }
        }

        private async Task Verify(IDictionary<string, (string Reader, EKeyState State)> readers)
        {
            foreach (var name in readers.ToArray()
                .Where(x => x.Value.State != EKeyState.Verified)
                .Select(x => x.Key))
            {
                try
                {
                    var state = await Verify(name) ? EKeyState.Verified : EKeyState.Connected;

                    if (state == EKeyState.Verified)
                    {
                        var drive = GetEKeyDrive();

                        if (drive != null)
                        {
                            SetEKeyVerified(true, drive);
                        }
                        else
                        {
                            state = EKeyState.Connected;
                        }
                    }

                    readers[name] = (name, state);
                }
                catch (SmartCardException ex)
                {
                    Logger.Error($"Unable to verify programs on card reader ({name}), {ex.Message}", ex);
                    readers[name] = (name, EKeyState.Disconnected);
                }
                catch (AggregateException ex) when (SmartCardError(ex))
                {
                    Logger.Error($"Unable to verify programs on card reader ({name})", ex);
                    readers[name] = (name, EKeyState.Disconnected);
                }
            }
        }

        private async Task<bool> Verify(string name)
        {
            using var reader = await SmartCardReader.FromName(name);
            using var connection = await reader.Connect(ShareMode.Exclusive, Protocol.T0);
            return _verificationPrograms.Any(program => program.Run(connection, _shutdown.Token));
        }

        private Task Unverify(IReadOnlyDictionary<string, (string Reader, EKeyState State)> readers)
        {
            if (readers.All(x => x.Value.State != EKeyState.Verified))
            {
                SetEKeyVerified(false, null);
            }

            return Task.CompletedTask;
        }

        private static string GetEKeyDrive()
        {
            return DriveInfo.GetDrives()
                .Where(x => x.IsReady && x.DriveType == DriveType.Removable && x.VolumeLabel == EKeyVolumeLabel)
                .Select(x => x.Name)
                .FirstOrDefault();
        }

        private static bool TaskCancelled(AggregateException ex)
        {
            return ex?.InnerExceptions.Any(e => e is TaskCanceledException or OperationCanceledException) ?? false;
        }

        private static bool SmartCardError(AggregateException ex)
        {
            return ex?.InnerExceptions.Any(e => e is SmartCardException) ?? false;
        }
    }
}
namespace Aristocrat.Monaco.Application.Detection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Detection;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using log4net;

    public class DeviceDetection : IDeviceDetection
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private CancellationTokenSource _tokenSource;

        public DeviceDetection()
            : this(ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        public DeviceDetection(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public string Name => typeof(OperatorLockupResetService).Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(IDeviceDetection) };

        public void Initialize()
        {
        }

        public void BeginDetection(IEnumerable<DeviceType> devices)
        {
            Logger.Debug($"BeginDetection of {string.Join(", ", devices)}");
            _tokenSource = new CancellationTokenSource();

            Task.Run(() => DetectionTask(), _tokenSource.Token);
        }

        public void CancelDetection()
        {
            Logger.Debug("BeginDetection");
            _tokenSource.Cancel();
        }

        private void DetectionTask()
        {
            //
            // THIS IS A PLACEHOLDER, TO BE FLESHED OUT LATER
            //
            Task.Delay(5000).ContinueWith(
                _ =>
                {
                    _eventBus.Publish(new DeviceDetectionCompletedEvent());
                });
        }
    }
}

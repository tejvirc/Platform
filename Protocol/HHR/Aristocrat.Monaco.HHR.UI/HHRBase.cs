namespace Aristocrat.Monaco.Hhr.UI
{
    using System.Reflection;
    using System.Threading;
    using Application.Contracts;
    using Common.Container;
    using Kernel;
    using log4net;
    using SimpleInjector;
    using Extensions;

    [ProtocolCapability(
        protocol:CommsProtocol.HHR,
        isValidationSupported:false,
        isFundTransferSupported:false,
        isProgressivesSupported:true,
        isCentralDeterminationSystemSupported:true)]
    public class HHRBase : BaseRunnable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);

        private Container _container = new Container();

        protected override void OnInitialize()
        {
            Logger.Debug($"Initializing {nameof(HHRBase)}");
            _container.AddResolveUnregisteredType(typeof(HHRBase).FullName, Logger);
            _container.Initialize();
            _container.ConfigureConsumers();
            _container.Verify();

            Logger.Debug($"Initialized {nameof(HHRBase)}");
        }

        protected override void OnRun()
        {
            Logger.Debug($"Running {nameof(HHRBase)} service.");

            var start = false;

            if (_shutdownEvent != null)
            {
                start = !_shutdownEvent.WaitOne(0);
            }

            if (start)
            {
                _shutdownEvent.WaitOne();
            }

            Logger.Debug($"Stopped {nameof(HHRBase)} service.");
        }

        protected override void OnStop()
        {
            Logger.Debug($"Stopping {nameof(HHRBase)} service.");
            _shutdownEvent.Set();
            Logger.Debug($"{nameof(HHRBase)} service stopped.");
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            Logger.Debug($"Disposing {nameof(HHRBase)} service.");
            base.Dispose(disposing);
            if (disposing)
            {
                _shutdownEvent?.Dispose();
                _container?.Dispose();
            }
            _shutdownEvent = null;
            _container = null;
            Logger.Debug($"Disposed {nameof(HHRBase)} service.");
        }
    }
}
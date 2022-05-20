namespace Aristocrat.Monaco.G2S.Security
{
    using System;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.CertificateManager;
    using Common.CertificateManager.Models;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Provides a mechanism to monitor the state of the PKI infrastructure including the enrollment status and state of an
    ///     enrolled certificate.
    /// </summary>
    public class CertificateMonitor : ICertificateMonitor, IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly TimeSpan UpdateInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan RetryInterval = TimeSpan.FromMinutes(5);

        private readonly ICertificateService _certificateService;
        private readonly IEventBus _eventBus;
        private readonly object _sync = new object();

        private DateTime _checkStatus = DateTime.UtcNow;
        private bool _disposed;

        private CancellationTokenSource _enrollCertificateCancellationToken;
        private CancellationTokenSource _statusMonitorCancellationToken;

        private Timer _timer;

        public CertificateMonitor(ICertificateService certificateService, IEventBus eventBus)
        {
            _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _timer = new Timer(CheckStatus);
        }

        public void Start()
        {
            CheckStatus(null);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_enrollCertificateCancellationToken != null)
                {
                    _enrollCertificateCancellationToken.Cancel();
                    _enrollCertificateCancellationToken.Dispose();
                }

                if (_statusMonitorCancellationToken != null)
                {
                    _statusMonitorCancellationToken.Cancel();
                    _statusMonitorCancellationToken.Dispose();
                }

                lock (_sync)
                {
                    if (_timer != null)
                    {
                        _timer.Dispose();
                        _timer = null;
                    }
                }
            }

            _enrollCertificateCancellationToken = null;
            _statusMonitorCancellationToken = null;

            lock (_sync)
            {
                _timer = null;
            }

            _disposed = true;
        }

        private void CheckStatus(object context)
        {
            try
            {
                // If SCEP is disabled we can simply bail
                var configuration = _certificateService.GetConfiguration();
                if (configuration == null || !configuration.ScepEnabled)
                {
                    return;
                }

                var now = DateTime.UtcNow;

                if (now >= _checkStatus && configuration.OcspEnabled)
                {
                    CheckCertificateStatus();
                }

                if (now >= _certificateService.NextRenewal())
                {
                    Renew();
                }
                else if (now >= _certificateService.NextExchange())
                {
                    Exchange(null);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to check certificate status", e);
            }
            finally
            {
                lock (_sync)
                {
                    _timer?.Change(UpdateInterval, Timeout.InfiniteTimeSpan);
                }
            }
        }

        private void CheckCertificateStatus()
        {
            GetCertificateStatusResult status;

            try
            {
                status = _certificateService.GetCertificateStatus();
            }
            catch (Exception e)
            {
                Logger.Warn("Unable to get the certificate's OCSP status", e);
                return;
            }

            if (status != null)
            {
                if (status.NextUpDateTime > DateTime.UtcNow)
                {
                    _checkStatus = status.NextUpDateTime ?? DateTime.UtcNow + RetryInterval;
                }

                Logger.Debug($"Certificate status will be checked at {_checkStatus} - current status is {status.Status}");
            }
            else
            {
                Logger.Warn("Unable to get the certificate's OCSP status.");
            }
        }

        private void Renew()
        {
            Logger.Info("Attempting to renew the current certificate");

            try
            {
                var action = _certificateService.Renew();

                ProcessCertificateEnrollResult(
                    action,
                    result =>
                    {
                        if (result.Status == CertificateRequestStatus.Denied || result.Status == CertificateRequestStatus.Error)
                        {
                            return;
                        }

                        if (DateTime.UtcNow >= result.Certificate.NotBefore.ToUniversalTime())
                        {
                            Exchange(result.Certificate);
                        }
                        else
                        {
                            _certificateService.InstallCertificate(result.Certificate);
                        }
                    });
            }
            catch (Exception ex)
            {
                Logger.Error("Renewal failed from the certificate monitor", ex);
            }
        }

        private void Exchange(X509Certificate2 certificate)
        {
            Logger.Info("Exchanging the current certificate");

            _certificateService.Exchange(certificate);
        }

        private void ProcessCertificateEnrollResult(
            CertificateActionResult result,
            Action<CertificateActionResult> onEnrolled = null)
        {
            _enrollCertificateCancellationToken?.Cancel(false);
            _enrollCertificateCancellationToken?.Dispose();
            _enrollCertificateCancellationToken = null;

            if (result != null)
            {
                switch (result.Status)
                {
                    case CertificateRequestStatus.Error:
                        Logger.Error("Failed to enroll certificate");
                        break;
                    case CertificateRequestStatus.Enrolled:
                        Logger.Debug("Enrolled certificate");

                        if (result.Certificate != null)
                        {
                            Logger.Info(
                                $"Installing enrolled certificate with thumbprint {result.Certificate.Thumbprint}");

                            if (onEnrolled == null)
                            {
                                _certificateService.InstallCertificate(result.Certificate, true);
                            }
                            else
                            {
                                onEnrolled.Invoke(result);
                            }
                        }
                        else
                        {
                            Logger.Error("Certificate instance does not exist.");
                        }

                        break;
                    case CertificateRequestStatus.Pending:
                        var delay = TimeSpan.FromSeconds(
                            _certificateService.GetConfiguration().ScepManualPollingInterval);

                        Logger.Info($"Certificate status will be checked again in {delay}");

                        _enrollCertificateCancellationToken = new CancellationTokenSource();

                        Task.Delay(delay, _enrollCertificateCancellationToken.Token)
                            .ContinueWith(
                                task =>
                                {
                                    if (!task.IsCanceled)
                                    {
                                        CheckStatus(result.RequestData, result.SigningCertificate, onEnrolled);
                                    }
                                });

                        break;
                    case CertificateRequestStatus.Denied:
                        Logger.Error("Certificate enrollment was denied");
                        break;
                }

                _eventBus.Publish(new CertificateStatusUpdatedEvent(result.Status));
            }
            else
            {
                Logger.Warn("Unable to process certificate enrollment response.");
                _eventBus.Publish(new CertificateStatusUpdatedEvent(CertificateRequestStatus.Error));
            }
        }

        private void CheckStatus(
            byte[] requestData,
            X509Certificate2 signingCertificate,
            Action<CertificateActionResult> onEnrolled = null)
        {
            var result = _certificateService.Poll(requestData, signingCertificate);

            ProcessCertificateEnrollResult(result, onEnrolled);
        }
    }
}
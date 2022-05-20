namespace Aristocrat.Monaco.Sas.Progressive
{
    using System;
    using System.Timers;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Kernel;

    /// <inheritdoc cref="IProgressiveHitExceptionProvider"/>
    public sealed class ProgressiveHitExceptionProvider : IProgressiveHitExceptionProvider, IDisposable
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _propertiesManager;
        private readonly Timer _timer;

        /// <summary>
        ///     Creates a new instance of the ProgressiveHitExceptionProvider. 
        /// </summary>
        /// <param name="exceptionHandler">The sas exception handler</param>
        /// <param name="propertiesManager">The properties manager</param>
        public ProgressiveHitExceptionProvider(ISasExceptionHandler exceptionHandler, IPropertiesManager propertiesManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _timer = new Timer(ProgressiveConstants.ProgressiveHitExceptionCycleTime) { AutoReset = false };
            _timer.Elapsed += TimerOnElapsed;
        }

        /// <inheritdoc />
        public void StartReportingSasProgressiveHit()
        {
            _exceptionHandler.ReportException(
                new GenericExceptionBuilder(GeneralExceptionCode.SasProgressiveLevelHit));
            _timer.Stop();
            _timer.Start();
        }

        /// <inheritdoc />
        public void StopSasProgressiveHitReporting()
        {
            _timer.Stop();
            _exceptionHandler.RemoveException(new GenericExceptionBuilder(GeneralExceptionCode.SasProgressiveLevelHit));
        }

        /// <inheritdoc />
        public void ReportNonSasProgressiveHit()
        {
            if (_propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).NonSasProgressiveHitReporting)
            {
                // This is a 6.03 Exception
                _exceptionHandler.ReportException(
                    new GenericExceptionBuilder(GeneralExceptionCode.NonSasProgressiveLevelHit));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _timer?.Dispose();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            StartReportingSasProgressiveHit();
        }
    }
}
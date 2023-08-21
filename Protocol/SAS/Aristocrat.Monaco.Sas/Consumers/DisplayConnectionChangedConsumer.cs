namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Cabinet.Contracts;
    using Application.Contracts;
    using Kernel;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Display;

    /// <summary>
    ///     Handles the <see cref="DisplayConnectionChangedEvent"/> to report a general tilt message to SAS
    /// </summary>
    public class DisplayConnectionChangedConsumer : Consumes<DisplayConnectionChangedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Creates a DisplayDisconnectedConsumer instance
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="propertiesManager">An instance of <see cref="IPropertiesManager"/></param>
        public DisplayConnectionChangedConsumer(
            ISasExceptionHandler exceptionHandler,
            IPropertiesManager propertiesManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        private bool IsTopperOverlayRedirecting => (bool)_propertiesManager.GetProperty(ApplicationConstants.IsTopperOverlayRedirecting, false);

        /// <inheritdoc />
        public override void Consume(DisplayConnectionChangedEvent evt)
        {
            if (!IsTopperOverlayRedirecting)
            {
                return;
            }

            if ((!evt.IsConnected && evt.Display != DisplayRole.Topper)
                || (evt.IsConnected && evt.Display == DisplayRole.Topper))
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.GeneralTilt));
            }
        }
    }
}
namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Events;

    /// <summary>
    ///     Handles the <see cref="Hardware.Contracts.Reel.Events.HardwareReelFaultEvent" /> event for reels.
    /// </summary>
    public class ReelFaultedConsumer : Consumes<HardwareReelFaultEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelFaultedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public ReelFaultedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(HardwareReelFaultEvent theEvent)
        {
            if (theEvent.Fault == ReelFaults.None)
            {
                return;
            }

            switch (theEvent.ReelId)
            {
                case 1:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.Reel1Tilt));
                    break;
                case 2:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.Reel2Tilt));
                    break;
                case 3:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.Reel3Tilt));
                    break;
                case 4:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.Reel4Tilt));
                    break;
                case 5:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.Reel5Tilt));
                    break;
                default:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.ReelTilt));
                    break;
            }
        }
    }
}
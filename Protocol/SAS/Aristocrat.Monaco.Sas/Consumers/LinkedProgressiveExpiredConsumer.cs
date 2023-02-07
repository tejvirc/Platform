namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using System.Linq;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;

    /// <summary>
    ///     Consumes the <see cref="LinkedProgressiveExpiredEvent"/> and figures out whether
    ///     or not we need to post exception 53.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class LinkedProgressiveExpiredConsumer: IProtocolProgressiveEventHandler
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the LinkedProgressiveExpiredConsumer class
        /// </summary>
        /// <param name="handler">The exception handler used for reporting sas exceptions</param>
        /// <param name="propertiesManager">The properties manager</param>
        public LinkedProgressiveExpiredConsumer(
            ISasExceptionHandler handler,
            IPropertiesManager propertiesManager)
        {
            _exceptionHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <summary>
        ///     Consumes a linked progressive expired event and reports the relevant exception.
        /// </summary>
        /// <param name="theEvent">The expired event to being consumed</param>
        public void Consume(LinkedProgressiveExpiredEvent theEvent)
        {
            // Per Larry the "Sas Man", exception 53 should only be reported if there are no existing expired levels per link;
            // that is, if we have a level expiration and there are already expired levels for the group id we manage,
            // do not post this this exception again.
            var groupId = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                .ProgressiveGroupId;

            if (theEvent.PreExistingExpiredLevels.Any(x => x.ProgressiveGroupId == groupId))
            {
                return;
            }

            if (theEvent.NewlyExpiredLevels.Any(x => x.ProgressiveGroupId == groupId))
            {
                _exceptionHandler.ReportException(
                    new GenericExceptionBuilder(GeneralExceptionCode.NoProgressiveInformationHasBeenReceivedFor5Seconds));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public void HandleProgressiveEvent<T>(T @event)
        {
            Consume(@event as LinkedProgressiveExpiredEvent);
        }
    }
}
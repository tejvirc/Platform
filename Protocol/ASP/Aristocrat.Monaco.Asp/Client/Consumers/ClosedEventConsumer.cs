namespace Aristocrat.Monaco.Asp.Client.Consumers
{
    using Contracts;
    using Hardware.Contracts.Door;
    using System;

    /// <summary>
    ///     Handles the <see cref="ClosedEvent" /> event.
    /// </summary>
    public class ClosedEventConsumer : Consumes<ClosedEvent>
    {
        private readonly ILogicSealDataSource _logicSealDataSource;
        private readonly IDoorsDataSource _doorsDataSource;

        /// <summary>
        ///     Creates a ClosedEventConsumer instance
        /// </summary>
        /// <param name="logicSealDataSource">An instance of <see cref="ILogicSealDataSource"/></param>
        /// <param name="doorsDataSource">An instance of <see cref="IDoorsDataSource"/></param>
        public ClosedEventConsumer(
            ILogicSealDataSource logicSealDataSource
            , IDoorsDataSource doorsDataSource)
        {
            _logicSealDataSource = logicSealDataSource ?? throw new ArgumentNullException(nameof(logicSealDataSource));
            _doorsDataSource = doorsDataSource ?? throw new ArgumentNullException(nameof(doorsDataSource));
        }

        public override void Consume(ClosedEvent theEvent)
        {
            _logicSealDataSource.HandleEvent(theEvent);
            _doorsDataSource.OnDoorStatusChanged(theEvent);
        }
    }
}

namespace Aristocrat.Monaco.Asp.Client.Consumers
{
    using Contracts;
    using Hardware.Contracts.Door;
    using System;

    /// <summary>
    ///     Handles the <see cref="OpenEvent" /> event.
    /// </summary>
    public class OpenEventConsumer : Consumes<OpenEvent>
    {
        private readonly ILogicSealDataSource _logicSealDataSource;
        private readonly IDoorsDataSource _doorsDataSource;

        /// <summary>
        ///     Creates a OpenEventConsumer instance
        /// </summary>
        /// <param name="logicSealDataSource">An instance of <see cref="ILogicSealDataSource"/></param>
        /// <param name="doorsDataSource"></param>
        public OpenEventConsumer(
            ILogicSealDataSource logicSealDataSource
            , IDoorsDataSource doorsDataSource)
        {
            _logicSealDataSource = logicSealDataSource ?? throw new ArgumentNullException(nameof(logicSealDataSource));
            _doorsDataSource = doorsDataSource ?? throw new ArgumentNullException(nameof(doorsDataSource));
        }

        public override void Consume(OpenEvent theEvent)
        {
            _logicSealDataSource.HandleEvent(theEvent);
            _doorsDataSource.OnDoorStatusChanged(theEvent);
        }
    }
}

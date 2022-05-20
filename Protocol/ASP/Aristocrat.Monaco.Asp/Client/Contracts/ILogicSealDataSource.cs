namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using Hardware.Contracts.Door;

    /// <summary>
    ///     Interface for Logic Seal Data Source that sets the logic seal status
    /// </summary>
    public interface ILogicSealDataSource
    {
        /// <summary>
        ///     Handle open and closed door events
        /// </summary>
        /// <param name="theEvent"></param>
        void HandleEvent(DoorBaseEvent theEvent);
    }
}

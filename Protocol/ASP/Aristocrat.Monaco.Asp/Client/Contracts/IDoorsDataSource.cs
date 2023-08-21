namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using Hardware.Contracts.Door;

    /// <summary>
    ///     Interface for Doors Data Source
    /// </summary>
    public interface IDoorsDataSource
    {
        /// <summary>
        ///  Handle Door Status Changed Event
        /// </summary>
        /// <param name="doorEvent"></param>
        void OnDoorStatusChanged(DoorBaseEvent doorEvent);

        /// <summary>
        /// Handle Door Open Metered Event
        /// </summary>
        /// <param name="doorEvent"></param>
        void OnDoorOpenMeterChanged(DoorOpenMeteredEvent doorEvent);
    }
}
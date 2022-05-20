namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     Manages the reserve state service
    /// </summary>
    public interface IReserveService : IService
    {
        /// <summary>
        ///     Indicates whether machine is reserved or not
        /// </summary>
        bool IsMachineReserved { get; }

        /// <summary>
        ///     This reserves the machine
        /// </summary>
        /// <returns>true if successful, false otherwise</returns>
        bool ReserveMachine();

        /// <summary>
        ///     This exits the reserve
        /// </summary>
        /// <returns>true if successful, false otherwise</returns>
        bool ExitReserveMachine();

        /// <summary>
        ///     Indicates whether machine can be reserved or not
        ///     by checking various conditions
        /// </summary>
        bool CanReserveMachine { get; }
    }
}
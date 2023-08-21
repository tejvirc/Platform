namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Defines methods to control handpay overlay animations
    /// </summary>
    public interface IHandpayRuntimeFlagsHelper
    {
        /// <summary>
        ///     Sets the runtime flags based on whether we can
        ///     show animations or not in the current platform state.
        /// </summary>
        void SetHandpayRuntimeLockupFlags();
    }
}
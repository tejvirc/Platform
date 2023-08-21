namespace Aristocrat.Monaco.Gaming.Contracts.Payment
{
    using Kernel;

    /// <summary>
    ///     An interface that we can use to register and find a <see cref="IPaymentDeterminationHandler"/>.
    /// </summary>
    public interface IPaymentDeterminationProvider : IService
    {
        /// <summary>
        ///     The IPaymentDeterminationHandler that is currently used to detect and split large wins.
        /// </summary>
        IPaymentDeterminationHandler Handler { get; set; }

        /// <summary>
        ///     The IBonusPaymentDeterminationHandler that is currently used to identify pay method for bonus payout.
        /// </summary>
        IBonusPaymentDeterminationHandler BonusHandler { get; set; }

        /// <summary>
        ///     Used to unregister bonus handler 
        /// </summary>
        void UnregisterBonusHandler();
    }
}
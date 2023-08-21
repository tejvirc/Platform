namespace Aristocrat.Monaco.Gaming.Payment
{
    using Gaming.Contracts.Payment;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Implements the <see cref="IPaymentDeterminationProvider"/> interface.
    /// </summary>
    public class PaymentDeterminationProvider : IPaymentDeterminationProvider
    {
        /// <inheritdoc />
        public IPaymentDeterminationHandler Handler { get; set; }

        public IBonusPaymentDeterminationHandler BonusHandler { get; set; }

        public void UnregisterBonusHandler()
        {
            BonusHandler = null;
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPaymentDeterminationProvider) };

        /// <inheritdoc />
        public void Initialize() { }
    }
}
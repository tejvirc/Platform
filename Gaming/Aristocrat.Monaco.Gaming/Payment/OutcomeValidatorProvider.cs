namespace Aristocrat.Monaco.Gaming.Payment
{
    using System;
    using System.Collections.Generic;
    using Gaming.Contracts.Payment;

    /// <summary>
    ///     Implements the <see cref="IOutcomeValidatorProvider"/> interface.
    /// </summary>
    public class OutcomeValidatorProvider : IOutcomeValidatorProvider
    {
        /// <inheritdoc />
        public IOutcomeValidator Handler { get; set; }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IOutcomeValidatorProvider) };

        /// <inheritdoc />
        public void Initialize() { }
    }
}
namespace Aristocrat.Monaco.Gaming.Contracts.Payment
{
    using Kernel;

    /// <summary>
    ///     An interface that we can use to register and find a <see cref="IOutcomeValidator"/>.
    /// </summary>
    public interface IOutcomeValidatorProvider : IService
    {
        /// <summary>
        ///     The IOutcomeValidator that is currently used to validate game outcomes.
        /// </summary>
        IOutcomeValidator Handler { get; set; }
    }
}

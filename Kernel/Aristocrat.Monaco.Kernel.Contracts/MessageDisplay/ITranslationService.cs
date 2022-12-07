namespace Aristocrat.Monaco.Kernel.Contracts.MessageDisplay
{
    /// <summary>
    /// The type of culture provider
    /// </summary>
    public enum CultureProviderType
    {
        /// <summary>
        ///     Used to get current culture for operator.
        /// </summary>
        Operator = 0,
        /// <summary>
        ///     Used to target the operator culture provider.
        /// </summary>
        OperatorTicket = 1,
        /// <summary>
        ///     Used to target the player culture provider.
        /// </summary>
        Player = 2,

        /// <summary>
        ///     Used to target the player ticket culture provider.
        /// </summary>
        PlayerTicket = 3,

        /// <summary>
        ///     Used to target the currency culture provider.
        /// </summary>
        Currency = 4
    }

    /// <summary>
    /// The service gets the resource string from selected culture provider
    /// </summary>
    public interface ITranslationService : IService
    {
        /// <summary>
        /// 
        /// </summary>
        string GetString(string key, CultureProviderType providerType);
    }
}
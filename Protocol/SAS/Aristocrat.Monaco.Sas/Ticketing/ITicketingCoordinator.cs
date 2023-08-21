namespace Aristocrat.Monaco.Sas.Ticketing
{
    using Storage.Models;
    using Storage.Repository;

    /// <summary>
    ///     An interface through which all ticketing activities can be coordinated
    /// </summary>
    public interface ITicketingCoordinator : IStorageDataProvider<TicketStorageData>
    {
        /// <summary>
        /// Gets the cashable ticket expiration in days. 9999 means it never expires.
        /// </summary>
        /// <returns>The cashable ticket expiration.</returns>
        ulong TicketExpirationCashable { get; }

        /// <summary>
        /// Gets the restricted ticket expiration in days. 9999 means it never expires.
        /// </summary>
        /// <returns>The restricted ticket expiration.</returns>
        ulong TicketExpirationRestricted { get; }

        /// <summary>
        ///     Gets the default restricted ticket expiration date. 9999 means it never expires.
        /// </summary>
        ulong DefaultTicketExpirationRestricted { get; }

        /// <summary>
        ///     Called when the validation configuration is cancelled
        /// </summary>
        void ValidationConfigurationCancelled();

        /// <summary>
        ///     Called when the restricted credit balance reaches zero
        /// </summary>
        void RestrictedCreditsZeroed();
    }
}
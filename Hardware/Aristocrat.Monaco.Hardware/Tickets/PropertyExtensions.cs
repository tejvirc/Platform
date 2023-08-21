namespace Aristocrat.Monaco.Hardware.Tickets
{
    using Contracts.Ticket;

    /// <summary>
    ///     Definition of the PropertyExtensions class.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>Checks whether given property boolean value is set.</summary>
        /// <param name="ticket">The ticket.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>True if boolean property is set.</returns>
        public static bool IsPropertySet(this Ticket ticket, string propertyName)
        {
            if (ticket == null || propertyName == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(ticket[propertyName]);
        }
    }
}
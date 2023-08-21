namespace Aristocrat.G2S.Client.Devices.v21
{
    using System.Linq;
    using Protocol.v21;

    /// <summary>
    ///     General protocol extensions for the v21 schema.
    /// </summary>
    public static class ProtocolExtensions
    {
        /// <summary>
        ///     Creates a deviceList for a device that can be used in an event report.
        /// </summary>
        /// <param name="this">The device.</param>
        /// <param name="status">The device statuses (cabinetStatus, commsStatus, etc.)</param>
        /// <returns>An event report compatible deviceList.</returns>
        public static deviceList1 DeviceList(this IDevice @this, object status)
        {
            return new deviceList1
            {
                statusInfo = new[]
                    { new statusInfo { deviceClass = @this.PrefixedDeviceClass(), deviceId = @this.Id, Item = status } }
            };
        }

        /// <summary>
        ///     Creates a transactionList for a device that can be used in an event report.
        /// </summary>
        /// <param name="this">The device.</param>
        /// <param name="transactions">The transactions</param>
        /// <returns>An event report compatible transactionList.</returns>
        public static transactionList TransactionList(this IDevice @this, params object[] transactions)
        {
            return new transactionList
            {
                transactionInfo = transactions.Select(transaction =>
                    new transactionInfo
                    {
                        deviceClass = @this.PrefixedDeviceClass(),
                        deviceId = @this.Id,
                        Item = transaction
                    }).ToArray()
            };
        }
    }
}

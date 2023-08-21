namespace Aristocrat.Monaco.G2S.Handlers
{
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Set of common verification methods for commands.
    /// </summary>
    public static class Sanction
    {
        /// <summary>
        ///     Verifies if the device restricted to owner and guests.
        /// </summary>
        /// <typeparam name="TDevice">The type of the device.</typeparam>
        /// <param name="deviceConnector">The device connector.</param>
        /// <param name="command">The command.</param>
        /// <returns>Result fo verification.</returns>
        public static Task<Error> OwnerAndGuests<TDevice>(IDeviceConnector deviceConnector, ClassCommand command)
            where TDevice : IDevice
        {
            return ValidateRestrictions<TDevice>(
                deviceConnector,
                command,
                CommandRestrictions.RestrictedToOwnerAndGuests);
        }

        /// <summary>
        ///     Verifies if the device restricted to owner only.
        /// </summary>
        /// <typeparam name="TDevice">The type of the device.</typeparam>
        /// <param name="deviceConnector">The egm.</param>
        /// <param name="command">The command.</param>
        /// <returns>Result fo verification.</returns>
        public static Task<Error> OnlyOwner<TDevice>(IDeviceConnector deviceConnector, ClassCommand command)
            where TDevice : IDevice
        {
            return ValidateRestrictions<TDevice>(deviceConnector, command, CommandRestrictions.RestrictedToOwner);
        }

        private static Task<Error> ValidateRestrictions<TDevice>(
            IDeviceConnector connector,
            ClassCommand command,
            CommandRestrictions restrictions)
            where TDevice : IDevice
        {
            var device = connector.GetDevice<TDevice>(command.IClass.deviceId);

            return Task.FromResult(command.Validate(device, restrictions));
        }
    }
}
namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using Communications;
    using Diagnostics;

    /// <summary>
    ///     Extension methods for the ClassCommand class.
    /// </summary>
    public static class ClassCommandExtensions
    {
        /// <summary>
        ///     Checks for an expired ClassCommand using the provided comm behavior
        /// </summary>
        /// <param name="this">The command to check</param>
        /// <param name="behavior">The comm behavior</param>
        /// <returns>Returns true if the command is expired, or false</returns>
        public static bool IsExpired(this ClassCommand @this, ICommBehavior behavior)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (behavior == null)
            {
                throw new ArgumentNullException(nameof(behavior));
            }

            switch (behavior.TimeToLiveBehavior)
            {
                case TimeToLiveBehavior.Strict:
                    // timeToLive of 0 means never expire
                    if (@this.IClass.timeToLive != 0)
                    {
                        if (@this.Received.ToUniversalTime().AddMilliseconds(@this.IClass.timeToLive) < DateTime.UtcNow)
                        {
                            SourceTrace.TraceWarning(
                                G2STrace.Source,
                                @"CommunicationsDevice.VerifyApplication : Received Expired Command
		EgmId : {0}
		CommandId : {1}
		SentTime : {2}
		ReceivedTime : {3}
		TimeToLive : {4}",
                                @this.EgmId,
                                @this.CommandId,
                                @this.IClass.dateTime,
                                @this.Received,
                                @this.IClass.timeToLive);

                            return true;
                        }
                    }

                    break;
            }

            return false;
        }

        /// <summary>
        ///     Validates the command against the provided device for a set of standard conditions
        /// </summary>
        /// <param name="this">The class command</param>
        /// <param name="device">The device</param>
        /// <param name="restrictions">Checks the access rights for the command based on the applied restriction.</param>
        /// <returns>Returns an error if the command fails validation, or null</returns>
        public static Error Validate(this ClassCommand @this, IDevice device, CommandRestrictions restrictions)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (device == null)
            {
                return new Error(ErrorCode.G2S_APX003);
            }

            if (@this.IsExpired(device.Queue))
            {
                return new Error(ErrorCode.G2S_APX011);
            }
             
            Error error = null;

            switch (restrictions)
            {
                case CommandRestrictions.RestrictedToOwner:
                    if (!device.IsOwner(@this.HostId))
                    {
                        error = new Error(ErrorCode.G2S_APX010);
                    }

                    break;
                case CommandRestrictions.RestrictedToOwnerAndGuests:
                    if (!device.IsOwner(@this.HostId) && !device.IsGuest(@this.HostId))
                    {
                        error = new Error(ErrorCode.G2S_APX012);
                    }

                    break;
            }

            return error;
        }
    }
}

namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;

    /// <summary>
    ///     Get verification status by device arguments
    /// </summary>
    public class GetVerificationStatusByDeviceArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetVerificationStatusByDeviceArgs" /> class.
        /// </summary>
        /// <param name="deviceId">Device identifier assigned to an instance of a device within a class by the EGM</param>
        /// <param name="verificationId">Verification identifier</param>
        public GetVerificationStatusByDeviceArgs(int deviceId, long verificationId)
        {
            if (verificationId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(verificationId), @"Must be more than zero");
            }

            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId), @"Must be more than zero");
            }

            DeviceId = deviceId;
            VerificationId = verificationId;
        }

        /// <summary>
        ///     Gets device identifier
        /// </summary>
        public int DeviceId { get; }

        /// <summary>
        ///     Gets verification identifier
        /// </summary>
        public long VerificationId { get; }
    }
}
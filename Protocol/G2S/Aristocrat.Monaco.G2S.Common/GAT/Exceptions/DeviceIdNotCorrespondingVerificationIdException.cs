namespace Aristocrat.Monaco.G2S.Common.GAT.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Device id not corresponding verification id exception
    /// </summary>
    [Serializable]
    public class DeviceIdNotCorrespondingVerificationIdException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceIdNotCorrespondingVerificationIdException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DeviceIdNotCorrespondingVerificationIdException(string message)
            : base(message)
        {
        }
    }
}
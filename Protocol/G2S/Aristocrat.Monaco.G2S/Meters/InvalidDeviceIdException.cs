namespace Aristocrat.Monaco.G2S.Meters
{
    using System;

    /// <summary>
    ///     Invalid device Id exception
    /// </summary>
    [Serializable]
    public class InvalidDeviceIdException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidDeviceIdException" /> class.
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        public InvalidDeviceIdException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}
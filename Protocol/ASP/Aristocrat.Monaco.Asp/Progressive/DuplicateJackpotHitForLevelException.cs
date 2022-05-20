namespace Aristocrat.Monaco.Asp.Progressive
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Exception thrown when jackpot hit event received for a progressive
    ///     level that already has an active jackpot being processed
    /// </summary>
    [Serializable]
    public class DuplicateJackpotHitForLevelException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DuplicateJackpotHitForLevelException" /> class.
        /// </summary>
        public DuplicateJackpotHitForLevelException()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DuplicateJackpotHitForLevelException" /> class.
        /// </summary>
        /// <param name="message">message</param>
        public DuplicateJackpotHitForLevelException(string message) : base(message)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DuplicateJackpotHitForLevelException" /> class.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="innerException">innerException</param>
        public DuplicateJackpotHitForLevelException(string message, Exception innerException) : base(message, innerException)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DuplicateJackpotHitForLevelException" /> class.
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="context">context</param>
        protected DuplicateJackpotHitForLevelException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
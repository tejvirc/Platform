namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Data class to hold the response of a single value
    /// </summary>
    public class LongPollReadSingleValueResponse<T> : LongPollResponse
    {
        /// <summary>
        ///     Creates an instance of the LongPollReadSingleValueResponse class
        /// </summary>
        /// <typeparamref name="T">The type of the data value</typeparamref>
        /// <param name="value">The value to store in the data</param>
        public LongPollReadSingleValueResponse(T value)
        {
            Data = value;
        }

        /// <summary>
        ///     Gets or sets the data being requested
        /// </summary>
        public T Data { get; set; }
    }
}

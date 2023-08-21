namespace Aristocrat.Monaco.Common.Validation
{
    /// <summary>
    ///     Validation error
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationError" /> class.
        /// </summary>
        /// <param name="propertyName">Invalid property name</param>
        /// <param name="message">Error message</param>
        public ValidationError(string propertyName, string message)
        {
            PropertyName = propertyName;
            Message = message;
        }

        /// <summary>
        ///     Gets invalid property name
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        ///     Gets error message
        /// </summary>
        public string Message { get; }
    }
}
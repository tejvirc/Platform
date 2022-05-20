namespace Aristocrat.Monaco.Common.Models
{
    using Validation;

    /// <summary>
    ///     Save entity result
    /// </summary>
    public class SaveEntityResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SaveEntityResult" /> class.
        /// </summary>
        /// <param name="successful">The result of operation defining whether it is success or not</param>
        public SaveEntityResult(bool successful)
        {
            Successful = successful;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SaveEntityResult" /> class.
        /// </summary>
        /// <param name="successful">Boolean true or false</param>
        /// <param name="validationResult">Parameter that defines whether the passed result of operation is success or not</param>
        public SaveEntityResult(bool successful, ValidationResult validationResult)
        {
            Successful = successful;
            ValidationResult = validationResult;
        }

        /// <summary>
        ///     Gets a value indicating whether the provided result of operation is success or not
        /// </summary>
        public bool Successful { get; }

        /// <summary>
        ///     Gets validation result
        /// </summary>
        public ValidationResult ValidationResult { get; }
    }
}
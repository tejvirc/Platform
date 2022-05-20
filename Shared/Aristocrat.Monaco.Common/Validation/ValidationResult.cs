namespace Aristocrat.Monaco.Common.Validation
{
    using System.Collections.Generic;

    /// <summary>
    ///     Validation result
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationResult" /> class.
        /// </summary>
        /// <param name="isValid">Parameter that defines whether the passed result of operation is success or not</param>
        /// <param name="errors">Errors list</param>
        public ValidationResult(bool isValid, IEnumerable<ValidationError> errors)
        {
            IsValid = isValid;
            Errors = errors;
        }

        /// <summary>
        ///     Gets a value indicating whether the provided result of operation is success or not
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        ///     Gets validation errors list
        /// </summary>
        public IEnumerable<ValidationError> Errors { get; }
    }
}
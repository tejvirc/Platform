namespace Aristocrat.Monaco.Common.Validation
{
    using System.Linq;

    /// <summary>
    ///     Validation result extension
    /// </summary>
    public static class ValidationResultExtensions
    {
        /// <summary>
        ///     Converts to common validation result.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <returns>The <see cref="ValidationResult" /></returns>
        public static ValidationResult ConvertToCommonValidationResult(
            this FluentValidation.Results.ValidationResult validationResult)
        {
            var commonValidationResult =
                new ValidationResult(
                    validationResult.IsValid,
                    validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)));

            return commonValidationResult;
        }
    }
}
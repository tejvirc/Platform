namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Validators
{
    using System;
    using FluentValidation.Validators;

    /// <summary>
    ///     Server location validator
    /// </summary>
    public class ServerLocationValidator : PropertyValidator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ServerLocationValidator" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public ServerLocationValidator(string errorMessage)
            : base(errorMessage)
        {
        }

        /// <summary>
        ///     Returns true if server location is valid.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        ///     <c>true</c> if the specified context is valid; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context?.PropertyValue == null)
            {
                return false;
            }

            var location = context.PropertyValue.ToString();

            return Uri.TryCreate(location, UriKind.Absolute, out var uri) && IsSchemeValid(uri);
        }

        private static bool IsSchemeValid(Uri address)
        {
            if (string.Compare(address.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }

            return string.Compare(address.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
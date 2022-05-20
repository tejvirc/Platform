namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Validators
{
    using Application.Contracts.Localization;
    using FluentValidation;
    using Localization.Properties;
    using Models;

    /// <summary>
    ///     Certificate configuration validator
    /// </summary>
    public class PkiConfigurationValidator : AbstractValidator<PkiConfiguration>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PkiConfigurationValidator" /> class.
        /// </summary>
        public PkiConfigurationValidator()
        {
            When(
                x => x.ScepEnabled,
                () =>
                {
                    RuleFor(x => x.CertificateManagerLocation)
                        .SetValidator(
                            new ServerLocationValidator(
                                Localizer.For(CultureFor.Operator).FormatString(
                                    ResourceKeys.ServerLocationInvalidFormatErrorMessageTemplate,
                                    "CertificateMgrLocation")));
                });

            When(
                x => x.OcspEnabled,
                () =>
                {
                    RuleFor(x => x.CertificateStatusLocation)
                        .SetValidator(
                            new ServerLocationValidator(
                                Localizer.For(CultureFor.Operator).FormatString(
                                    ResourceKeys.ServerLocationInvalidFormatErrorMessageTemplate,
                                    "CertificateStatusLocation")));
                });

            // TODO: provide in configuration and uncomment rule

            // RuleFor(x => x.ScepPreSharedSecret).NotEmpty();
            RuleFor(x => x.ScepManualPollingInterval).GreaterThan(0);

            RuleFor(x => x.OcspMinimumPeriodForOffline).InclusiveBetween((short)0, short.MaxValue);

            RuleFor(x => x.OcspReAuthenticationPeriod).InclusiveBetween((short)1, short.MaxValue);

            RuleFor(x => x.OcspAcceptPreviouslyGoodCertificatePeriod)
                .InclusiveBetween((short)1, short.MaxValue)
                .WithMessage(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OcspAcceptPrevCertPeriodMinGreateMoreOcspReauthPeriodMinErrorMessage));
        }
    }
}
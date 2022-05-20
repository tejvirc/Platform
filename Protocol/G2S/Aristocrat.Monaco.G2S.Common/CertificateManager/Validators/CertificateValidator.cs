namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Validators
{
    using System;
    using FluentValidation;
    using Models;
    using Monaco.Common.Validation;

    /// <summary>
    ///     Certificate entity validator
    /// </summary>
    public class CertificateValidator : AbstractValidator<Certificate>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateValidator" /> class.
        /// </summary>
        public CertificateValidator()
        {
            When(
                x => x != null,
                () =>
                {
                    RuleSet(ValidationRuleType.EditEntity.ToString(), () => RuleFor(x => x.Id).GreaterThan(0));

                    RuleSet(ValidationRuleType.AddEntity.ToString(), () => { RuleFor(x => x.Id).Equal(0); });

                    RuleSet(
                        ValidationRuleType.Common.ToString(),
                        () =>
                        {
                            RuleFor(x => x.VerificationDate).NotNull().GreaterThan(DateTime.MinValue);
                            RuleFor(x => x.Thumbprint).NotEmpty();
                        });
                });
        }
    }
}
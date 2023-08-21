namespace Aristocrat.Monaco.G2S.Common.GAT.Validators
{
    using Application.Contracts.Localization;
    using FluentValidation;
    using Kernel.Contracts.Components;
    using Localization.Properties;

    /// <summary>
    ///     Component validator
    /// </summary>
    public class ComponentValidator : AbstractValidator<Component>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentValidator" /> class.
        /// </summary>
        public ComponentValidator()
        {
            RuleFor(x => x.ComponentId).NotEmpty();
            RuleFor(x => x.Description).Length(1, 256).When(x => !string.IsNullOrWhiteSpace(x.Description));
            RuleFor(x => x.Path).SetValidator(
                new ComponentPathValidator(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PathIsInvalidErrorMessage)));
        }
    }
}

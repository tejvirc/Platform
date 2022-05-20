namespace Aristocrat.Monaco.G2S.Common.GAT.Validators
{
    using FluentValidation;
    using Storage;

    /// <summary>
    ///     GAT special function entity validator
    /// </summary>
    public class GatSpecialFunctionEntityValidator : AbstractValidator<GatSpecialFunction>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatSpecialFunctionEntityValidator" /> class.
        /// </summary>
        public GatSpecialFunctionEntityValidator()
        {
            RuleFor(x => x.Feature).NotEmpty();
        }
    }
}
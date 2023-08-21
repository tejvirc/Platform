namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using Kernel.Contracts.Components;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Models;
    using Monaco.Common.Validation;
    using Validators;

    /// <summary>
    ///     Save component handler
    /// </summary>
    public class SaveComponentHandler : IParametersFuncHandler<Component, SaveEntityResult>
    {
        private readonly IComponentRegistry _componentRegistry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SaveComponentHandler" /> class.
        /// </summary>
        /// <param name="componentRepository">The component repository.</param>
        public SaveComponentHandler(IComponentRegistry componentRepository)
        {
            _componentRegistry = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        }

        /// <summary>
        ///     Executes the specified component entity.
        /// </summary>
        /// <param name="parameter">The component entity.</param>
        /// <returns>Save result.</returns>
        public SaveEntityResult Execute(Component parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var componentValidator = new ComponentValidator();

            var validationResult = componentValidator.Validate(parameter);

            if (validationResult.IsValid)
            {
                _componentRegistry.Register(parameter);

                return new SaveEntityResult(true);
            }

            return new SaveEntityResult(false, validationResult.ConvertToCommonValidationResult());
        }
    }
}

namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Models;
    using Monaco.Common.Storage;
    using Monaco.Common.Validation;
    using Storage;
    using Validators;

    /// <summary>
    ///     Save special function handler
    /// </summary>
    public class SaveSpecialFunctionHandler : IParametersFuncHandler<GatSpecialFunction, SaveEntityResult>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IGatSpecialFunctionRepository _gatSpecialFunctionRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SaveSpecialFunctionHandler" /> class.
        /// </summary>
        /// <param name="gatSpecialFunctionRepository">The gat special function repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public SaveSpecialFunctionHandler(
            IGatSpecialFunctionRepository gatSpecialFunctionRepository,
            IMonacoContextFactory contextFactory)
        {
            _gatSpecialFunctionRepository = gatSpecialFunctionRepository ??
                                            throw new ArgumentNullException(nameof(gatSpecialFunctionRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <summary>
        ///     Executes the specified gat special function entity.
        /// </summary>
        /// <param name="parameter">The gat special function entity.</param>
        /// <returns>Save result.</returns>
        public SaveEntityResult Execute(GatSpecialFunction parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var gatSpecialFunctionEntityValidator = new GatSpecialFunctionEntityValidator();

            var validationResult = gatSpecialFunctionEntityValidator.Validate(parameter);

            if (validationResult.IsValid)
            {
                using (var context = _contextFactory.Create())
                {
                    if (parameter.Id == 0)
                    {
                        _gatSpecialFunctionRepository.Add(context, parameter);
                    }
                    else
                    {
                        _gatSpecialFunctionRepository.Update(context, parameter);
                    }
                }

                return new SaveEntityResult(true);
            }

            return new SaveEntityResult(false, validationResult.ConvertToCommonValidationResult());
        }
    }
}
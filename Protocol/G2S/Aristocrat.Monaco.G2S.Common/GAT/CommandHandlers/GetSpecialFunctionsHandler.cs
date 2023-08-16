namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Get special functions handler
    /// </summary>
    public class GetSpecialFunctionsHandler : IFuncHandler<IEnumerable<GatSpecialFunction>>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IGatSpecialFunctionRepository _gatSpecialFunctionRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetSpecialFunctionsHandler" /> class.
        /// </summary>
        /// <param name="gatSpecialFunctionRepository">The gat special function repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public GetSpecialFunctionsHandler(
            IGatSpecialFunctionRepository gatSpecialFunctionRepository,
            IMonacoContextFactory contextFactory)
        {
            _gatSpecialFunctionRepository = gatSpecialFunctionRepository ??
                                            throw new ArgumentNullException(nameof(gatSpecialFunctionRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>List of GAT special function.</returns>
        public IEnumerable<GatSpecialFunction> Execute()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _gatSpecialFunctionRepository.GetAll(context).ToList();
            }
        }
    }
}
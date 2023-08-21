namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using ExpressMapper;
    using Monaco.Common.Storage;

    /// <summary>
    ///     An implementation of ICommandBuilder&lt;IOptionConfigDevice, optionChangeStatus&gt;.
    /// </summary>
    public class OptionChangeStatusCommandBuilder : ICommandBuilder<IOptionConfigDevice, optionChangeStatus>
    {
        private readonly IOptionChangeLogRepository _changeLogRepository;
        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionChangeStatusCommandBuilder" /> class.
        /// </summary>
        /// <param name="changeLogRepository">The change log repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public OptionChangeStatusCommandBuilder(
            IOptionChangeLogRepository changeLogRepository,
            IMonacoContextFactory contextFactory)
        {
            _changeLogRepository = changeLogRepository ?? throw new ArgumentNullException(nameof(changeLogRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public async Task Build(IOptionConfigDevice device, optionChangeStatus command)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var optionChangeLog = _changeLogRepository.GetByTransactionId(context, command.transactionId);

                Mapper.Map(optionChangeLog, command);
                command.applyCondition = optionChangeLog.ApplyCondition.ToG2SString();
                command.disableCondition = optionChangeLog.DisableCondition.ToG2SString();

                if (optionChangeLog.AuthorizeItems != null)
                {
                    var authorizeItems =
                        optionChangeLog.AuthorizeItems.Select(Mapper.Map<ConfigChangeAuthorizeItem, authorizeStatus>)
                            .ToArray();
                    if (authorizeItems.Any())
                    {
                        command.authorizeStatusList = new authorizeStatusList
                        {
                            authorizeStatus = authorizeItems
                        };
                    }
                }
            }

            await Task.CompletedTask;
        }
    }
}
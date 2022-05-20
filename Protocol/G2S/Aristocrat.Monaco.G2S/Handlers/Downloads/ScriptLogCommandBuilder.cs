namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager.Storage;
    using Data.Model;
    using ExpressMapper;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class ScriptLogCommandBuilder : ICommandBuilder<IDownloadDevice, scriptLog>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IScriptRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptLogCommandBuilder" /> class.
        /// </summary>
        /// <param name="repository">An instance of IScriptRepository.</param>
        /// <param name="contextFactory">DB context factory.</param>
        public ScriptLogCommandBuilder(IScriptRepository repository, IMonacoContextFactory contextFactory)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public async Task Build(IDownloadDevice device, scriptLog command)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (command.scriptId <= 0)
            {
                throw new ArgumentException(nameof(command.scriptId));
            }

            var script = _repository.GetScriptByScriptId(_contextFactory.Create(), command.scriptId);

            command.transactionId = script.TransactionId;
            command.scriptId = script.ScriptId;
            if (script.StartDateTime.HasValue)
            {
                command.startDateTime = script.StartDateTime.Value;
                command.startDateTimeSpecified = true;
            }

            if (script.EndDateTime.HasValue)
            {
                command.endDateTime = script.EndDateTime.Value;
                command.endDateTimeSpecified = true;
            }

            command.disableCondition = script.DisableCondition.ToG2SString();
            command.applyCondition = script.ApplyCondition.ToG2SString();
            command.reasonCode = script.ReasonCode;
            command.scriptStatus = (t_scriptStates)Enum.Parse(
                typeof(t_scriptStates),
                script.State.G2SScriptStatesFromScriptStates());
            command.scriptException = script.ScriptException;

            if (script.AuthorizeDateTime.HasValue)
            {
                command.authorizeDateTime = script.AuthorizeDateTime.Value;
                command.authorizeDateTimeSpecified = true;
            }

            if (script.CompletedDateTime.HasValue)
            {
                command.completeDateTime = script.CompletedDateTime.Value;
                command.completeDateTimeSpecified = true;
            }

            command.deviceId = script.DeviceId;
            command.logSequence = script.Id;
            command.transactionId = script.TransactionId;

            if (script.AuthorizeItems != null)
            {
                var authorizeItems = script.AuthorizeItems.Select(Mapper.Map<ConfigChangeAuthorizeItem, authorizeStatus>).ToArray();
                if (authorizeItems.Length > 0)
                {
                    command.authorizeStatusList = new authorizeStatusList
                    {
                        authorizeStatus = authorizeItems
                    };
                }
            }

            using (TextReader reader = new StringReader(script.CommandData))
            {
                command.commandStatusList =
                    (commandStatusList)new XmlSerializer(typeof(commandStatusList)).Deserialize(reader);
            }

            await Task.CompletedTask;
        }
    }
}
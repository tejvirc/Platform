namespace Aristocrat.Sas.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using log4net;

    /// <summary>
    ///     This class creates the long poll parsers
    /// </summary>
    public class SasLongPollParserFactory : ISasParserFactory
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<LongPoll, ILongPollParser> _parsers =
            new Dictionary<LongPoll, ILongPollParser>();
        private readonly SasLongPollParser<LongPollResponse, LongPollData> _unknownParser = new UnhandledLongPollParser();

        /// <inheritdoc/>>
        public ILongPollParser GetParserForLongPoll(LongPoll longPoll)
        {
            return _parsers.TryGetValue(longPoll, out var handler) ? handler : _unknownParser;
        }

        /// <inheritdoc/>>
        public void LoadParsers(SasClientConfiguration configuration, Assembly assembly = null)
        {
            // if the caller didn't specify an assembly, assume the assembly we're in
            if (assembly is null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            // use reflection to create parsers of type SasLongPollParser and add them to the dictionary
            var parsers =
                from type in assembly.GetExportedTypes()
                where (type.BaseType != null && type.BaseType.IsGenericType ||
                       type.BaseType != null && type.BaseType.BaseType != null &&
                       type.BaseType.BaseType.IsGenericType) &&
                      (type.GetConstructor(Type.EmptyTypes) != null ||
                       type.GetConstructor(new[] { typeof(SasClientConfiguration) }) != null)
                select type;

            foreach (var parser in parsers)
            {
                var attribute = (SasAttribute)Attribute.GetCustomAttribute(parser, typeof(SasAttribute));
                if (attribute is null || !ThisLongPollHandledByThisClient(configuration, attribute))
                {
                    continue;
                }

                ILongPollParser longPollParser;
                if (parser.GetConstructor(new[] { typeof(SasClientConfiguration) }) != null)
                {
                    longPollParser = (ILongPollParser)Activator.CreateInstance(parser, configuration);
                }
                else
                {
                    longPollParser = (ILongPollParser)Activator.CreateInstance(parser);
                }

                Logger.Debug($"Adding parser {parser.Name} with command {(byte)(longPollParser.Command):X2}");
                LoadSingleParser(longPollParser);
            }

            Logger.Debug($"Added {_parsers.Count} parsers");
        }

        /// <inheritdoc/>>
        public void LoadSingleParser(ILongPollParser parser)
        {
            _parsers.Add(parser.Command, parser);
        }

        /// <inheritdoc/>>
        public void InjectHandler(object handler, LongPoll longPoll)
        {
            GetParserForLongPoll(longPoll).InjectHandler(handler);
        }

        private static bool ThisLongPollHandledByThisClient(SasClientConfiguration configuration, SasAttribute attribute)
        {
            return attribute.Group == SasGroup.PerClientLoad ||
                   configuration.HandlesAft && attribute.Group == SasGroup.Aft ||
                   configuration.HandlesEft && attribute.Group == SasGroup.Eft ||
                   configuration.HandlesLegacyBonusing && attribute.Group == SasGroup.LegacyBonus ||
                   configuration.HandlesValidation && attribute.Group == SasGroup.Validation ||
                   configuration.HandlesGeneralControl && attribute.Group == SasGroup.GeneralControl ||
                   configuration.HandlesProgressives && attribute.Group == SasGroup.Progressives;
        }
    }
}

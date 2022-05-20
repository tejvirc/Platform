namespace Aristocrat.Sas.Client
{
    using System.Reflection;

    public interface ISasParserFactory
    {
        /// <summary>
        ///     Gets a parser for the given long poll command.
        /// </summary>
        /// <param name="longPoll">The long poll being parsed</param>
        /// <returns>The long poll parser for the command</returns>
        ILongPollParser GetParserForLongPoll(LongPoll longPoll);

        /// <summary>
        ///     Uses reflection to load parsers at runtime.
        /// </summary>
        /// <param name="configuration">The configuration information</param>
        /// <param name="assembly">The assembly calling this method. Used to determine what
        /// assembly to get the parser classes from.</param>
        void LoadParsers(SasClientConfiguration configuration, Assembly assembly = null);

        /// <summary>
        ///     Loads a single parser into our list of parsers
        /// </summary>
        /// <param name="parser">The parser to load</param>
        void LoadSingleParser(ILongPollParser parser);

        /// <summary>
        ///     Allows the platform to inject a handler for a given long poll into the engine
        /// </summary>
        /// <param name="handler">The handler for the long poll</param>
        /// <param name="longPoll">The long poll to handle</param>
        void InjectHandler(object handler, LongPoll longPoll);
    }
}
namespace Aristocrat.Monaco.Gaming.Contracts.PlayerInfoDisplay
{
    using System;

    /// <summary>
    ///     Arguments to show what command is requested
    /// </summary>
    public class CommandArgs : EventArgs
    {
        /// <summary>
        ///     Constructor to create Player Information Display command
        /// </summary>
        /// <param name="commandType"></param>
        public CommandArgs(CommandType commandType)
        {
            CommandType = commandType;
        }
        /// <summary>
        ///     Command type requested
        /// </summary>
        public CommandType CommandType { get; }

    }
}
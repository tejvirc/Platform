﻿namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>Class to handle output events.</summary>
    [Serializable]
    public class OutputEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OutputEvent" /> class.
        /// </summary>
        /// <param name="id">Physical IO that was handled.</param>
        /// <param name="action">Action that was performed. For example light on/off, bell ringing/not ringing.</param>
        public OutputEvent(int id, bool action)
        {
            Id = id;
            Action = action;
        }

        /// <summary>Gets a value indicating whether Id is set.</summary>
        public int Id { get; }

        /// <summary>Gets a value indicating whether Action is set.</summary>
        public bool Action { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Id={1}, Action={2}]",
                GetType().Name,
                Id,
                Action);
        }
    }
}
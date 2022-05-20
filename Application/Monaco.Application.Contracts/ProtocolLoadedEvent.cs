namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     The <see cref="ProtocolLoadedEvent" /> is published after the configured protocols are intialized
    /// </summary>
    public class ProtocolLoadedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProtocolLoadedEvent" /> class.
        /// </summary>
        /// <param name="protocols">A list of initialized protocols</param>
        public ProtocolLoadedEvent(IEnumerable<string> protocols)
        {
            Protocols = protocols ?? throw new ArgumentNullException(nameof(protocols));
        }

        /// <summary>
        ///     Gets the list of initialized protocols
        /// </summary>
        public IEnumerable<string> Protocols { get; }
    }
}
namespace Aristocrat.G2S.Emdi.Host
{
    using Protocol.v21ext1b1;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Configuration for media display host
    /// </summary>
    public class HostConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostConfiguration"/> class.
        /// </summary>
        /// <param name="port">Port the media content will use to communicate with the host</param>
        public HostConfiguration(int port)
        {
            Port = port;

            Commands = SupportedCommands.Get();
            FunctionalGroups = Commands.Keys.ToList();
            Events = SupportedEvents.Get();
            Meters = SupportedMeters.Get();
        }

        /// <summary>
        /// Gets the port the media content will use to communicate with the host
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets the list of supported functional groups
        /// </summary>
        public IList<string> FunctionalGroups { get; }

        /// <summary>
        /// Gets the list of supported commands
        /// </summary>
        public IDictionary<string, List<string>> Commands { get; }

        /// <summary>
        /// Gets the list of supported events
        /// </summary>
        public IList<(string Code, string Text)> Events { get; }

        /// <summary>
        /// Gets the list of supported meters
        /// </summary>
        public IList<(string Name, t_meterTypes Type)> Meters { get; }
    }
}
namespace Aristocrat.Monaco.Application.Contracts.AuditTicketMonitor
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     This is a Mono.Addins ExtensionNode for audit ticket monitor configuration for a door trigger
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("Door")]
    public class DoorTriggerNode : ExtensionNode
    {
#pragma warning disable 0649
        [NodeAttribute("name", Required = true)] private string _name;
#pragma warning restore 0649

        /// <summary>
        ///     Gets the door name.
        /// </summary>
        public string Name => _name;
    }
}

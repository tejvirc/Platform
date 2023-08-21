namespace Aristocrat.Monaco.Application.Contracts.AuditTicketMonitor
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     This is a Mono.Addins ExtensionNode for audit ticket monitor configuration for triggers
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("Triggers")]
    [ExtensionNodeChild(typeof(DoorTriggerNode))]
    public class TriggersNode : ExtensionNode
    {
    }
}
////////////////////////////////////////////////////////////////////////////////////////////
namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Extension node used for extension point where components can
    ///     specify the tilt logger configuration.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("DoorMonitorConfiguration")]
    [ExtensionNodeChild(typeof(DoorOpenAlarmExtensionNode))]
    public class DoorMonitorConfigurationExtensionNode : ExtensionNode
    {
    }
}
////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="DeviceImplementationExtensionNode.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2013 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Extension node used for extension point where components
    ///     can specify a particular device implementation.
    /// </summary>
    [ExtensionNode("DeviceImplementation")]
    [CLSCompliant(false)]
    public class DeviceImplementationExtensionNode : TypeExtensionNode
    {
        /// <summary>
        ///     Gets or sets the device protocol name.
        /// </summary>
        [NodeAttribute]
        public string ProtocolName { get; set; }
    }
}
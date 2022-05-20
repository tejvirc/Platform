////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="DoorOpenAlarmExtensionNode.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2016 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;
    using Mono.Addins;

    /// <summary>
    ///     Extension node used for extension point where components can
    ///     specify the tilt logger configuration.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("DoorOpenAlarm")]
    [ExtensionNodeChild(typeof(FilePathExtensionNode))]
    public class DoorOpenAlarmExtensionNode : ExtensionNode
    {
        /// <summary>
        ///     Gets or sets the number of seconds before the door alarm is repeated.
        /// </summary>
        [NodeAttribute]
        public string RepeatSeconds { get; set; }

        /// <summary>
        ///     Gets or sets the number of times to play the sound file.
        /// </summary>
        [NodeAttribute]
        public string LoopCount { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the operator can cancel the door alarm.
        /// </summary>
        [NodeAttribute]
        public string OperatorCanCancel { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the door alarm sound should be stopped when the door is closed.
        /// </summary>
        [NodeAttribute]
        public string CanStopSoundWhenDoorIsClosed { get; set; }
    }
}
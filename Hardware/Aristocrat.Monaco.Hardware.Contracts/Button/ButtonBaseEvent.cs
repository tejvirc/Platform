////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="ButtonBaseEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2009-2010 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Hardware.Contracts.Button
{
    using System;
    using System.Globalization;
    using Kernel;
    using ProtoBuf;

    /// <summary>Class to handle button specific events. This class must be inherited from to use.</summary>
    [ProtoContract]
    public abstract class ButtonBaseEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ButtonBaseEvent" /> class.
        /// </summary>
        protected ButtonBaseEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ButtonBaseEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical ID of the button.</param>
        protected ButtonBaseEvent(int logicalId)
        {
            LogicalId = logicalId;
        }

        /// <summary>Gets a value indicating whether LogicalId is set.</summary>
        [ProtoMember(1)]
        public int LogicalId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [LogicalId={1}]",
                GetType().Name,
                LogicalId);
        }
    }
}
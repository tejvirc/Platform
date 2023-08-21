////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="HardMeterBaseEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2009-2010 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Hardware.Contracts.HardMeter
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>Class to handle <c>HardMeter</c> events.</summary>
    [Serializable]
    public abstract class HardMeterBaseEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardMeterBaseEvent" /> class.
        /// </summary>
        protected HardMeterBaseEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardMeterBaseEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical ID of the <c>HardMeter</c>.</param>
        protected HardMeterBaseEvent(int logicalId)
        {
            LogicalId = logicalId;
        }

        /// <summary>Gets a value indicating whether LogicalId is set.</summary>
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
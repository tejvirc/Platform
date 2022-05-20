////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="OffEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2009 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Hardware.Contracts.HardMeter
{
    using System;

    /// <summary>Definition of the HardMeterOffEvent class.</summary>
    [Serializable]
    public class OffEvent : HardMeterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OffEvent" /> class.
        /// </summary>
        public OffEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OffEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical <c>HardMeter</c> ID.</param>
        public OffEvent(int logicalId)
            : base(logicalId)
        {
        }
    }
}
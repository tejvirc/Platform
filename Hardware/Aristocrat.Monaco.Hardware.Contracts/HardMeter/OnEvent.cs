////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="OnEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2009 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Hardware.Contracts.HardMeter
{
    using System;

    /// <summary>Definition of the LightOnEvent class.</summary>
    [Serializable]
    public class OnEvent : HardMeterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OnEvent" /> class.
        /// </summary>
        public OnEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OnEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical light ID.</param>
        public OnEvent(int logicalId)
            : base(logicalId)
        {
        }
    }
}
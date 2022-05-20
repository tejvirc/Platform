////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="OffEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2009 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Hardware.Contracts.KeySwitch
{
    using System;

    /// <summary>Definition of the <c>KeySwitchOffEvent</c> class.</summary>
    [Serializable]
    public class OffEvent : KeySwitchBaseEvent
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
        /// <param name="logicalId">The logical key switch ID.</param>
        /// <param name="keySwitchName">The name of the key switch.</param>
        public OffEvent(int logicalId, string keySwitchName)
            : base(logicalId, keySwitchName)
        {
        }
    }
}
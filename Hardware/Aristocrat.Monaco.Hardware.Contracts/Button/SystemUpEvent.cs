////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="UpEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2009 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Hardware.Contracts.Button
{
    using System;

    /// <summary>Definition of the SystemUpEvent class.</summary>
    [Serializable]
    public class SystemUpEvent : UpEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemUpEvent" /> class.
        /// </summary>
        public SystemUpEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemUpEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical button ID.</param>
        /// <param name="isEnabled">The system button state.</param>
        public SystemUpEvent(int logicalId, bool isEnabled)
            : base(logicalId)
        {
            Enabled = isEnabled;
        }

        /// <summary>
        ///     State of the system button.
        /// </summary>
        public bool Enabled { get; set; }
    }
}
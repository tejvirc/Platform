////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="TimeUpdatedEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2010-2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     An event to notify that the system time has been updated.
    /// </summary>
    /// <remarks>
    ///     This event will be posted when the system time is updated successfully
    ///     through the <c>Update</c> of <c>ITime</c>. Any component which is sensitive to
    ///     the system time adjustment should consider handling this event.
    /// </remarks>
    [Serializable]
    public class TimeUpdatedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeUpdatedEvent" /> class.
        ///     A parameterless constructor is required for events that are sent from the
        ///     Key to Event converter.
        /// </summary>
        public TimeUpdatedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeUpdatedEvent" /> class.
        /// </summary>
        /// <param name="timeUpdate">The type of service removed.</param>
        public TimeUpdatedEvent(TimeSpan timeUpdate)
        {
            TimeUpdate = timeUpdate;
        }

        /// <summary>
        ///     Gets the amount of time that the system has been updated by.
        /// </summary>
        public TimeSpan TimeUpdate { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [TimeUpdate={1}]",
                GetType().Name,
                TimeUpdate);
        }
    }
}
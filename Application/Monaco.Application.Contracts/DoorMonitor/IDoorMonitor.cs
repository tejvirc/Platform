////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="IDoorMonitor.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2011-2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     An interface to provide the access to the logged message.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Through this interface, the already logged messages can be retrived
    ///         and the ongoing logged message will be notified to components which
    ///         hook up to the event declared in this interface.
    ///     </para>
    ///     <para>
    ///         The component implementing this interface must save the log messages
    ///         so that other components can retrieve them for display or for other
    ///         purposes. If the amount of messages exceeds the maxium, it should
    ///         retain the most recent ones.
    ///     </para>
    ///     <para>
    ///         There is an event defined. A component which is  interested in the live
    ///         log messages related to the doors should consider hooking up to this event.
    ///         The argument being passed along with this event is defined in
    ///         <see cref="DoorMonitorAppendedEventArgs" />
    ///     </para>
    ///     <example>
    ///         <code>
    ///        public class DoorLogHistory : Page
    ///        {
    ///          private EventHandler&lt;DoorMonitorAppendedEventArgs&gt; logEventHandler;
    ///
    ///          public void Page_Load()
    ///          {
    ///            // ...
    ///            logEventHandler = new EventHandler&lt;DoorMonitorAppendedEventArgs&gt;(DoorMonitorLogAppended);
    ///
    ///            // Get the IDoorMonitor service
    ///            IDoorMonitor doorLog = ...
    ///
    ///            doorLog.DoorMonitorAppended += logEventHandler;
    ///
    ///            // Populate the entries to the log presentation.
    ///            foreach (var entry in doorLog.Log)
    ///            {
    ///              DoorMonitorLogAppended(this, new DoorMonitorAppendedEventArgs(entry));
    ///            }
    ///          }
    ///
    ///          private void private void DoorMonitorLogAppended(object sender, DoorMonitorAppendedEventArgs e)
    ///          {
    ///            Dispatcher.BeginInvoke(new ThreadSwitchDelegate(AppendLogText), e);
    ///          }
    ///
    ///          private  void AppendLogText(DoorMonitorAppendedEventArgs message)
    ///          {
    ///            // Display the logged message.
    ///            // ...
    ///          }
    ///        }
    ///      </code>
    ///     </example>
    /// </remarks>
    public interface IDoorMonitor
    {
        /// <summary>
        ///     Gets or sets the maximum number of log messages the service should store.
        ///     If at the maximum, the service will remove the oldest message upon
        ///     receiving and storing a new message.  If set to a negative number, zero
        ///     will be used instead.
        /// </summary>
        int MaxStoredLogMessages { get; set; }

        /// <summary>
        ///     Gets the logged messages, ordered from oldest to newest.
        /// </summary>
        IList<DoorMessage> Log { get; }

        /// <summary>
        ///     Gets the doors mapped by their logical IDs.
        /// </summary>
        Dictionary<int, string> Doors { get; }

        /// <summary>
        ///     The event fired when the door monitor log is appended.
        /// </summary>
        /// <remarks>
        ///     Hook up to this event to recive the most recently appended message.
        /// </remarks>
        event EventHandler<DoorMonitorAppendedEventArgs> DoorMonitorAppended;


        /// <summary>
        ///     Gets doors with state.
        /// </summary>
        /// <returns>Gets logical doors.</returns>
        Dictionary<int, bool> GetLogicalDoors();

        /// <summary>Gets the localized door name for the given logical door ID.</summary>
        /// <param name="doorId">The logical door ID.</param>
        /// <param name="useDefaultCulture">If you don't care what the operator's culture is and only want default</param>
        /// <returns>The localized door name.</returns>
        string GetLocalizedDoorName(int doorId, bool useDefaultCulture = false);
    }

    #region DoorMessage struct

    /// <summary>
    ///     Organizes information about door events
    /// </summary>
    public struct DoorMessage
    {
        /// <summary>
        ///     Gets or sets the time that the event occurred
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        ///     Gets or sets the door id for the event
        /// </summary>
        public int DoorId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the door was opened
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the event passed validation (door closes only)
        /// </summary>
        public bool ValidationPassed { get; set; }

        /// <summary>
        ///     Overrides the Equality operator
        /// </summary>
        /// <param name="doorMessage1">first instance of dooreMessage Object</param>
        /// <param name="doorMessage2">second instance of doorMessage object</param>
        /// <returns> if they are equal</returns>
        public static bool operator ==(DoorMessage doorMessage1, DoorMessage doorMessage2)
        {
            return doorMessage1.DoorId == doorMessage2.DoorId && doorMessage1.IsOpen == doorMessage2.IsOpen &&
                   doorMessage1.Time == doorMessage2.Time &&
                   doorMessage1.ValidationPassed == doorMessage2.ValidationPassed;
        }

        /// <summary>
        ///     Overrides the inequality operator
        /// </summary>
        /// <param name="doorMessage1">first instance of dooreMessage Object</param>
        /// <param name="doorMessage2">second instance of doorMessage object</param>
        /// <returns>if they are not equal</returns>
        public static bool operator !=(DoorMessage doorMessage1, DoorMessage doorMessage2)
        {
            return !(doorMessage1 == doorMessage2);
        }

        /// <summary>
        ///     Compares Equality of the Objects
        /// </summary>
        /// <param name="obj">Passed object of the same type</param>
        /// <returns>returns if the object in the parameter holds the same values as the current instance.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is DoorMessage))
            {
                return false;
            }

            var target = (DoorMessage)obj;
            return this == target;
        }

        /// <summary>
        ///     GetHashCode computes the hashcode for this object instance
        /// </summary>
        /// <returns>computed hashcode </returns>
        public override int GetHashCode()
        {
            // Overflow is fine, just wrap
            unchecked
            {
                var hash = 17;

                hash = hash * 23 + DoorId.GetHashCode();
                hash = hash * 23 + IsOpen.GetHashCode();
                hash = hash * 23 + Time.GetHashCode();

                hash = hash * 23 + ValidationPassed.GetHashCode();
                return hash;
            }
        }
    }

    #endregion
}
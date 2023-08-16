////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="PrintButtonStatusEvent.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2016 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using Kernel;
    using ProtoBuf;
    using System;

    /// <summary>
    ///     An event for used to set the enabled value and status of the configurable print button.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is posted when any operator menu component needs to enable or disable the
    ///         configurable print button or display a printing staus on the MenuSelectionWindow.
    ///     </para>
    ///     <para>
    ///         The event should be handled by MenuSelectionWindow to enable or disable the configurable
    ///         print button and display a printing status specific to the page content.
    ///     </para>
    /// </remarks>
    [ProtoContract]
    public class PrintButtonStatusEvent : BaseEvent
    {

        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public PrintButtonStatusEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrintButtonStatusEvent" /> class.
        /// </summary>
        /// <param name="enabled">Indicates whether or not the print button is enabled.</param>
        public PrintButtonStatusEvent(bool enabled)
        {
            Enabled = enabled;
        }

        /// <summary>Gets a value indicating whether or not the print button is enabled.</summary>
        [ProtoMember(1)]
        public bool Enabled { get; }
    }
}
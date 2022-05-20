////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="ButtonDeckNavigatorEndedEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify that the button deck navigator has already finished its job.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The button deck navigator component is used by the configuration wizard and system
    ///         operator menu to select the display element. When they have completed all steps,
    ///         the navigator will be removed from the memory.
    ///     </para>
    ///     <para>
    ///         The event will be posted when the navigator is about to be disposed.
    ///     </para>
    ///     <para>
    ///         On XSpin, this event is digested by the component which manages the
    ///         button lights. Check the configuration file <c>LightAndAlarmCoordinatorMappings.addin</c>
    ///         for more. The button lights for navigation should be turned off when it is received by the
    ///         component.
    ///     </para>
    /// </remarks>
    [Serializable]
    public class ButtonDeckNavigatorEndedEvent : BaseEvent
    {
    }
}
////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="ButtonDeckNavigatorStartedEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify that the button deck navigator is starting to function.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The button deck navigator component is used by the configuration wizard and system
    ///         operator menu to select a display element. When either of them is started, the
    ///         navigator has to be loaded and initialized.
    ///     </para>
    ///     <para>
    ///         The event will be posted when the navigator is in the process of initialization.
    ///     </para>
    ///     <para>
    ///         On XSpin, this event is handled by the component which manages the
    ///         button lights. Check the configuration file <c>LightAndAlarmCoordinatorMappings.addin.xml</c>
    ///         for more. The button lights for navigation should be turned on when it is received by the
    ///         component.
    ///     </para>
    /// </remarks>
    [Serializable]
    public class ButtonDeckNavigatorStartedEvent : BaseEvent
    {
    }
}
﻿////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="MetersOperatorMenuExitedEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2011-2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Vgt.Client12.Application.OperatorMenu
{
    using System;
    using Aristocrat.Monaco.Kernel;
    using ProtoBuf;

    /// <summary>
    ///     An event to notify that the screen related to meters has been exited.
    /// </summary>
    /// <remarks>
    ///     In XSpin, this event is only handled by the component implementing
    ///     the backend protocol like SAS.
    /// </remarks>
    [ProtoContract]
    public class MetersOperatorMenuExitedEvent : BaseEvent
    {
    }
}
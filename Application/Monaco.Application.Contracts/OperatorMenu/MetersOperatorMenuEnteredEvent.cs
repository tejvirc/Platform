////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="MetersOperatorMenuEnteredEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2011-2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Vgt.Client12.Application.OperatorMenu
{
    using System;
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    ///     An event to notify that the screen related to meters has entered.
    /// </summary>
    /// <remarks>
    ///     On XSpin, this event is only handled by the component implementing
    ///     the backend protocol like SAS.
    /// </remarks>
    [Serializable]
    public class MetersOperatorMenuEnteredEvent : BaseEvent
    {
    }
}
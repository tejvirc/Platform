////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="SystemDisabledByOperatorEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2013 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify that the system has been disabled by operator.
    /// </summary>
    [Serializable]
    public class SystemDisabledByOperatorEvent : BaseEvent
    {
    }
}
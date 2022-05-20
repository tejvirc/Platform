////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="PeriodMetersClearedEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2010-2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify that the period meter values have been cleared.
    /// </summary>
    /// <remarks>
    ///     When a request to clear the period meters is received by the component managing
    ///     all meters, it will iterate all meter providers to ask each to clear its provided
    ///     meters' period value. This event will be posted when the iteration is done.
    /// </remarks>
    [Serializable]
    public class PeriodMetersClearedEvent : BaseEvent
    {
        /// <summary>
        /// Called when all periodic meters registered have been reset.
        /// </summary>
        public PeriodMetersClearedEvent()
        {

        }

        /// <summary>
        /// Called when all periodic meters associated with the provider with the same providerName has been reset.
        /// </summary>
        /// <param name="providerName"></param>
        public PeriodMetersClearedEvent(string providerName)
        {
            ProviderName = providerName;
        }

        /// <summary>
        /// The provider whose meters has been reset. If this is blank, all periodic meters were reset.
        /// </summary>
        public string ProviderName { get; set; }
    }
}
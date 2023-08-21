////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="MeterValueType.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2010-2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    /// <summary>
    ///     The type of meter value
    /// </summary>
    public enum MeterValueType
    {
        /// <summary>
        ///     Lifetime meter value
        /// </summary>
        Lifetime,

        /// <summary>
        ///     Period meter value
        /// </summary>
        Period,

        /// <summary>
        ///     Session meter value
        /// </summary>
        Session
    }
}
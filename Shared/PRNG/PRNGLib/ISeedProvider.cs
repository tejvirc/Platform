// <copyright file="ISeedProvider.cs" company="Aristocrat, Inc">
// Copyright (c) Aristocrat, Inc. All rights reserved.
// </copyright>

namespace PRNGLib
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     ISeedProvider
    /// </summary>
    public interface ISeedProvider : IDisposable
    {
        /// <summary>
        ///     GetValues
        /// </summary>
        /// <param name="values">List of values.</param>
        /// <param name="numberOfValues">Num values to fill in.</param>
        void GetValues(Collection<uint> values, int numberOfValues);
    }
}
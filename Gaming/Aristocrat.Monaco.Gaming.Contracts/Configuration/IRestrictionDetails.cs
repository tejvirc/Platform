namespace Aristocrat.Monaco.Gaming.Contracts.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines game configuration that can be used to restrict and ease game configuration
    /// </summary>
    public interface IRestrictionDetails
    {
        /// <summary>
        ///     Gets the configuration Id
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     Gets the name of the game configuration
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the maximum theoretical payback percentage for the configuration; a value of 0 (zero) indicates that the
        ///     attribute is not supported; otherwise, MUST be set to the maximum payback percentage of the game, which MUST be
        ///     greater than 0
        ///     (zero). For example, a value of 96371 represents a maximum payback percentage of 96.371%
        /// </summary>
        decimal MaximumPaybackPercent { get; }

        /// <summary>
        ///     Gets the minimum theoretical payback percentage for the configuration; a value of 0 (zero) indicates that the
        ///     attribute is not supported; otherwise, MUST be set to the minimum payback percentage for the game, which MUST be
        ///     greater than 0
        ///     (zero). For example, a value of 82452 represent
        /// </summary>
        decimal MinimumPaybackPercent { get; }

        /// <summary>
        ///     Gets the maximum number of denoms that can be enabled. If <c>null</c>, then the maximum is is not limited. For
        ///     example, with SingleDenom mode, this will be the value 1.
        /// </summary>
        int? MaxDenomsEnabled { get; }

        /// <summary>
        ///     Gets the minimum number of denoms which must be enabled.
        /// </summary>
        // ReSharper disable once UnusedMemberInSuper.Global
        int MinDenomsEnabled { get; } // This unused Property is needed for future use

        /// <summary>
        ///     Gets a value indicating whether or not the configuration is editable.  A value of true indicates the configuration
        ///     can be modified by the operator
        /// </summary>
        bool Editable { get; }

        /// <summary>
        ///     Gets a collection of available denom to paytable maps
        /// </summary>
        IEnumerable<IDenomToPaytable> Mapping { get; }
    }
}
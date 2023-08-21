namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Information about the lines played within the game.
    /// </summary>
    /// <remarks>
    ///     https://confy.aristocrat.com/display/ConfyOverhaulPOC/%5BBetlines%5D+Changing+Betlines+-+Info+and+Gap+Analysis
    /// </remarks>
    public class LineOption : IEquatable<LineOption>
    {
        /// <summary>
        ///     The unique identifier for LineOption.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     description determines the number of lines the option plays.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Lines determine which placements in the reel stops create a "win".
        /// </summary>
        public IEnumerable<Line> Lines { get; set; }

        /// <inheritdoc />
        public bool Equals(LineOption other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Name == other.Name;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(LineOption))
            {
                return false;
            }

            return Equals((LineOption)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        /// <summary>
        ///     Checks for equality.
        /// </summary>
        public static bool operator ==(LineOption left, LineOption right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Checks for inequality.
        /// </summary>
        public static bool operator !=(LineOption left, LineOption right)
        {
            return !Equals(left, right);
        }
    }
}
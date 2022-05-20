namespace Aristocrat.Monaco.Hardware.Contracts.TicketContent
{
    using System;
    using System.Collections;

    /// <summary>Indicates what comparison method to be used.</summary>
    public enum ComparisonType
    {
        /// <summary>Position comparison for portrait orientation.</summary>
        PositionComparisonPortrait,

        /// <summary>Position comparison for landscape orientation.</summary>
        PositionComparisonLandscape,

        /// <summary>Line comparison for portrait orientation.</summary>
        LineComparisonPortrait,

        /// <summary>Line comparison for landscape orientation.</summary>
        LineComparisonLandscape
    }

    /// <summary>
    ///     Definition of the Comparer class.
    ///     This class provides the methods for comparing the PrintableRegion objects.
    /// </summary>
    public class Comparer : IComparer
    {
        /// <summary>Initializes a new instance of the <see cref="Comparer" /> class.</summary>
        /// <param name="comparisonType">The comparison type.</param>
        /// <exception cref="ArgumentOutOfRangeException"><c>comparisonType</c> is out of range.</exception>
        public Comparer(ComparisonType comparisonType)
        {
            if (!Enum.IsDefined(typeof(ComparisonType), comparisonType))
            {
                throw new ArgumentOutOfRangeException(nameof(comparisonType));
            }

            ComparisonMethod = comparisonType;
        }

        /// <summary>Gets or sets ComparisonMethod.</summary>
        public ComparisonType ComparisonMethod { get; set; }

        /// <summary>Compare method for PrintableRegion object.</summary>
        /// <param name="x">The first object.</param>
        /// <param name="y">The second object.</param>
        /// <returns>The result of the comparison.</returns>
        /// <exception cref="ArgumentException">Object is not of type <see cref="PrintableRegion" />.</exception>
        public int Compare(object x, object y)
        {
            var leftRegion = x as PrintableRegion;
            var rightRegion = y as PrintableRegion;

            if (leftRegion == null)
            {
                throw new ArgumentException(@"Object is not of type PrintableRegion.", nameof(x));
            }

            if (rightRegion == null)
            {
                throw new ArgumentException(@"Object is not of type PrintableRegion.", nameof(y));
            }

            return leftRegion.CompareTo(rightRegion, ComparisonMethod);
        }
    }
}
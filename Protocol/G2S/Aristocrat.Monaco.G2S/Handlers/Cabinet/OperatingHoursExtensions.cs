namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Operations;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Extension methods for operating hours
    /// </summary>
    public static class OperatingHoursExtensions
    {
        /// <summary>
        ///     Converts a <see cref="OperatingHours" /> instance to a <see cref="operatingHours" />
        /// </summary>
        /// <param name="this">The <see cref="OperatingHours" /> instance to convert.</param>
        /// <returns>A <see cref="operatingHours" /> instance.</returns>
        public static operatingHours ToOperatingHours(this OperatingHours @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return new operatingHours
            {
                state = @this.Enabled ? t_operatingHoursState.GTK_enable : t_operatingHoursState.GTK_disable,
                time = @this.Time,
                weekday = (t_weekday)@this.Day
            };
        }

        /// <summary>
        ///     Converts a <see cref="operatingHours" /> instance to a <see cref="OperatingHours" />
        /// </summary>
        /// <param name="this">The <see cref="operatingHours" /> instance to convert.</param>
        /// <returns>A <see cref="OperatingHours" /> instance.</returns>
        public static OperatingHours ToOperatingHours(this c_operatingHours @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (@this.weekday == t_weekday.GTK_all)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(@this),
                    @this.weekday,
                    @"Weekday value of GTK_all cannot be converted.");
            }

            return new OperatingHours
            {
                Day = (DayOfWeek)@this.weekday,
                Time = @this.time,
                Enabled = @this.state == t_operatingHoursState.GTK_enable
            };
        }

        /// <summary>
        ///     Converts a <see cref="operatingHours" /> array to a <see cref="OperatingHours" /> collection.
        /// </summary>
        /// <param name="this">The <see cref="operatingHours" /> instance to convert.</param>
        /// <returns>A <see cref="OperatingHours" /> collection.</returns>
        public static IEnumerable<OperatingHours> ToOperatingHours(this operatingHours[] @this)
        {
            if (@this == null)
            {
                return Enumerable.Empty<OperatingHours>();
            }

            var result = new List<OperatingHours>();

            // If the weekday is set to all we need to expand the single day-based entry to the whole week
            foreach (var week in @this.Where(h => h.weekday == t_weekday.GTK_all))
            {
                result.AddRange(
                    Enumerable.Range(0, 7).Select(
                        day => new OperatingHours
                        {
                            Day = (DayOfWeek)day,
                            Time = week.time,
                            Enabled = week.state == t_operatingHoursState.GTK_enable
                        }));
            }

            result.AddRange(@this.Where(h => h.weekday != t_weekday.GTK_all).Select(h => h.ToOperatingHours()));

            return result.OrderBy(h => h.Day).ThenBy(h => h.Time);
        }

        /// <summary>
        ///     Converts a <see cref="OperatingHours" /> collection instance to a <see cref="OperatingHours" /> array.
        /// </summary>
        /// <param name="this">The <see cref="OperatingHours" /> collection to convert.</param>
        /// <returns>A <see cref="operatingHours" /> array.</returns>
        public static operatingHours[] ToOperatingHours(this IEnumerable<OperatingHours> @this)
        {
            return @this.Select(h => h.ToOperatingHours()).OrderBy(h => h.weekday).ThenBy(h => h.time).ToArray();
        }
    }
}
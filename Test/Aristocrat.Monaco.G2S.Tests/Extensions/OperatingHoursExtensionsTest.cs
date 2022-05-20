namespace Aristocrat.Monaco.G2S.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Operations;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Cabinet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OperatingHoursExtensionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenG2SOperatingHoursToOperatingHoursWeekdayIsAllExpectException()
        {
            var operatingHours = new operatingHours();
            operatingHours.weekday = t_weekday.GTK_all;
            operatingHours.ToOperatingHours();
        }

        [TestMethod]
        public void WhenG2SOperatingHoursToOperatingHoursWeekdayIsNotAllExpectSuccess()
        {
            foreach (var weekday in Enum.GetValues(typeof(t_weekday)))
            {
                var weekdayEnum = (t_weekday)weekday;

                if (weekdayEnum == t_weekday.GTK_all)
                {
                    continue;
                }

                var operatingHours = new operatingHours
                {
                    state = t_operatingHoursState.GTK_enable,
                    time = 200,
                    weekday = weekdayEnum
                };

                var convertedOperatingHours = operatingHours.ToOperatingHours();

                Assert.AreEqual((DayOfWeek)operatingHours.weekday, convertedOperatingHours.Day);
                Assert.AreEqual(operatingHours.time, convertedOperatingHours.Time);
                Assert.AreEqual(
                    operatingHours.state == t_operatingHoursState.GTK_enable,
                    convertedOperatingHours.Enabled);
            }
        }

        [TestMethod]
        public void WhenOperatingHoursToG2SOperatingHoursExpectSuccess()
        {
            var operatingHours = new OperatingHours
            {
                Day = DayOfWeek.Friday,
                Enabled = true,
                Time = 100
            };

            var convertedOperatingHours = operatingHours.ToOperatingHours();

            Assert.AreEqual((t_weekday)operatingHours.Day, convertedOperatingHours.weekday);
            Assert.AreEqual(operatingHours.Enabled, convertedOperatingHours.state == t_operatingHoursState.GTK_enable);
            Assert.AreEqual(operatingHours.Time, convertedOperatingHours.time);
        }

        [TestMethod]
        public void WhenOperatingHoursEnumerableToG2SOperatingHoursArrayExpectSuccess()
        {
            var first = new OperatingHours
            {
                Day = DayOfWeek.Friday,
                Enabled = true,
                Time = 200
            };

            var second = new OperatingHours
            {
                Day = DayOfWeek.Monday,
                Enabled = true,
                Time = 100
            };

            var operatingHours = new List<OperatingHours> { first, second };

            var convertedOperatingHours = operatingHours.ToOperatingHours();

            Assert.AreEqual(2, convertedOperatingHours.Length);

            var firstConverted = convertedOperatingHours[0];
            var secondConverted = convertedOperatingHours[1];

            Assert.AreEqual((t_weekday)first.Day, secondConverted.weekday);
            Assert.AreEqual(first.Enabled, secondConverted.state == t_operatingHoursState.GTK_enable);
            Assert.AreEqual(first.Time, secondConverted.time);

            Assert.AreEqual((t_weekday)second.Day, firstConverted.weekday);
            Assert.AreEqual(second.Enabled, firstConverted.state == t_operatingHoursState.GTK_enable);
            Assert.AreEqual(second.Time, firstConverted.time);
        }

        [TestMethod]
        public void WhenG2SOperatingHoursArrayToOperatingHoursEnumerableExpectSuccess()
        {
            var first = new operatingHours
            {
                state = t_operatingHoursState.GTK_enable,
                time = 200,
                weekday = t_weekday.GTK_all
            };

            var operatingHours = new[] { first };

            var convertedOperatingHours = operatingHours.ToOperatingHours();

            Assert.AreEqual(7, convertedOperatingHours.Count());

            var expectedWeekday = (t_weekday)0;
            foreach (var converted in convertedOperatingHours)
            {
                Assert.AreEqual(expectedWeekday, (t_weekday)converted.Day);
                Assert.AreEqual(first.time, converted.Time);
                Assert.AreEqual(first.state == t_operatingHoursState.GTK_enable, converted.Enabled);

                expectedWeekday++;
            }
        }
    }
}
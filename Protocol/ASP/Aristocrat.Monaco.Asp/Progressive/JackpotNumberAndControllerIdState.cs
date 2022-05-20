namespace Aristocrat.Monaco.Asp.Progressive
{
    using System;

    public class JackpotNumberAndControllerIdState : IComparable
    {
        public int? LevelId { get; set; }
        public long? JackpotNumber { get; set; }
        public int? JackpotControllerIdByteOne { get; set; }
        public int? JackpotControllerIdByteTwo { get; set; }
        public int? JackpotControllerIdByteThree { get; set; }

        /// <inheritdoc />
        public int CompareTo(object x)
        {
            var a = (JackpotNumberAndControllerIdState)x;

            return a.LevelId == LevelId && a.JackpotNumber == JackpotNumber &&
                   a.JackpotControllerIdByteOne == JackpotControllerIdByteOne &&
                   a.JackpotControllerIdByteTwo == JackpotControllerIdByteTwo &&
                   a.JackpotControllerIdByteThree == JackpotControllerIdByteThree ? 0 : 1;
        }
    }
}
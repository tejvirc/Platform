﻿namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;
    using static System.FormattableString;

    /// <summary>
    ///     Represents the properties of a Jackpot
    /// </summary>
    [Serializable]
    public class Jackpot : ICloneable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Jackpot" /> class.
        /// </summary>
        public Jackpot()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Jackpot" /> class.
        /// </summary>
        public Jackpot(int deviceId, int levelId, string levelName, long value)
        {
            DeviceId = deviceId;
            LevelId = levelId;
            LevelName = levelName;
            Value = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Jackpot" /> class.
        /// </summary>
        /// <param name="jackpot">The jackpot.</param>
        public Jackpot(Jackpot jackpot)
        {
            if (jackpot == null)
            {
                return;
            }

            DeviceId = jackpot.DeviceId;
            LevelId = jackpot.LevelId;
            LevelName = jackpot.LevelName;
            Value = jackpot.Value;
        }

        /// <summary>
        ///     Gets the progressive device identifier
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets the identifier of the level
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        ///     Gets the string identifier of the level
        /// </summary>
        public string LevelName { get; set; }

        /// <summary>
        ///     Gets the current jackpot value in millicents
        /// </summary>
        public long Value { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"Jackpot [{DeviceId} {LevelId} {Value}]");
        }
    }
}
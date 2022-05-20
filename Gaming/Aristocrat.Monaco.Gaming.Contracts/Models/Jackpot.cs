namespace Aristocrat.Monaco.Gaming.Contracts.Models
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
        public Jackpot(int deviceId, int levelId, long value)
        {
            DeviceId = deviceId;
            LevelId = levelId;
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
        ///     Gets the current jackpot value
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
namespace Aristocrat.Monaco.Hardware.Contracts.EdgeLighting
{
    using System.Collections.Generic;

    /// <summary>
    ///     Game edge light data
    /// </summary>
    public class GameEdgelightData
    {
        /// <summary>
        ///     Constructor for GameEdgelightData
        /// </summary>
        /// <param name="brightness">The brightness</param>
        /// <param name="lightData">The light data</param>
        public GameEdgelightData(int brightness, IReadOnlyDictionary<int, IEnumerable<byte>> lightData)
        {
            Brightness = brightness;
            LightData = lightData;
        }

        /// <summary>
        ///     The brightness
        /// </summary>
        public int Brightness { get; }

        /// <summary>
        ///     The light data
        /// </summary>
        public IReadOnlyDictionary<int, IEnumerable<byte>> LightData { get; }

        /// <summary>
        ///     The controlled strips
        /// </summary>
        public IEnumerable<int> ControlledStrips => LightData.Keys;
    }
}
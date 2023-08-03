namespace Aristocrat.Monaco.PackageManifest.Ati
{
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     Implementation of the GameSpecificOption manifest
    /// </summary>
    public class GameSpecificOptionManifest : IManifest<GameSpecificOptionConfig>
    {
        /// <inheritdoc />
        public GameSpecificOptionConfig Read(string file)
        {
            return File.Exists(file) ? Parse(file) : null;
        }

        private static GameSpecificOptionConfig Parse(string file)
        {
            GameSpecificOptionConfig config;

            try
            {
                config = ManifestUtilities.Parse<GameSpecificOptionConfig>(file);
            }
            catch (Exception)
            {
                throw;
            }

            Validate(config, file);

            return config;
        }

        private static void Validate(GameSpecificOptionConfig config, string file)
        {
            foreach (var g in config.GameToggleOptions.GameToggleOption)
            {
                if (!g.value.Equals("On", StringComparison.InvariantCulture) && !g.value.Equals("Off", StringComparison.InvariantCulture))
                {
                    throw new ArgumentOutOfRangeException($"GameToggleOption: {g.name}, value={g.value} is not valid, should be On or Off");
                }
            }

            foreach (var g in config.GameListOptions.GameListOption)
            {
                if (!g.List.Select(x => x.name).ToList().Contains(g.value))
                {
                    throw new ArgumentOutOfRangeException($"GameListOption: {g.name}, value={g.value} is not in the List in {file}");
                }
            }

            foreach (var g in config.GameNumberOptions.GameNumberOption)
            {
                if (g.maxValue < g.minValue)
                {
                    throw new ArgumentOutOfRangeException($"GameNumberOption: {g.name}, maxValue={g.maxValue} is less than MinValue={g.minValue} in {file}");
                }

                if (g.value < g.minValue)
                {
                    throw new ArgumentOutOfRangeException($"GameNumberOption: {g.name}, value={g.value} is less than MinValue={g.minValue} in {file}");
                }

                if (g.value > g.maxValue)
                {
                    throw new ArgumentOutOfRangeException($"GameNumberOption {g.name}, value={g.value} is larger then MaxValue={g.maxValue} in {file}");
                }
            }
        }

        /// <inheritdoc />
        public GameSpecificOptionConfig Read(Func<Stream> streamProvider)
        {
            throw new NotImplementedException();
        }
    }
}
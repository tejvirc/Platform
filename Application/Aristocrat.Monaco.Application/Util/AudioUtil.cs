
namespace Aristocrat.Monaco.Application.Util
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;

    /// <summary> Extension methods to Audio service to load and play sound </summary>
    public static class AudioUtil
    {
        private const byte DefaultAlertVolume = 100;
        private const int DefaultAlertDurationMs = 5000;
        private const int DefaultSoundLengthMs = 500;

        private static readonly Dictionary<string, int> DicFileNameToLoopCount = new Dictionary<string, int>();

        /// <summary> Load sound if provided. </summary>
        public static void LoadSound(this IAudio audioService, string soundFilePath)
        {
            if (!string.IsNullOrEmpty(soundFilePath) && !DicFileNameToLoopCount.ContainsKey(soundFilePath))
            {
                audioService.Load(Path.GetFullPath(soundFilePath));
                var loopCount = GetLoopCountForSound(audioService, soundFilePath);
                DicFileNameToLoopCount.Add(soundFilePath, loopCount);
            }
        }

        private static int GetLoopCountForSound(IAudio audioService, string soundFilePath)
        {
            var soundLength = audioService.GetLength(soundFilePath);
            var soundLengthMs = (soundLength != TimeSpan.Zero) ? soundLength.TotalMilliseconds : DefaultSoundLengthMs;
            var loopCount = (int)Math.Ceiling(DefaultAlertDurationMs / soundLengthMs);
            return loopCount;
        }

        /// <summary> Plays the sound file provided for specific duration </summary>
        public static void PlaySound(this IAudio audioService, IPropertiesManager properties, string soundFilePath)
        {
            if (!string.IsNullOrEmpty(soundFilePath))
            {

                if (!DicFileNameToLoopCount.ContainsKey(soundFilePath))
                {
                    audioService.LoadSound(soundFilePath);
                }
                int loopCount = DicFileNameToLoopCount[soundFilePath];

                var alertVolume = properties.GetValue(ApplicationConstants.AlertVolumeKey, DefaultAlertVolume);
                audioService.Play(soundFilePath, loopCount, alertVolume);
            }
        }
    }
}

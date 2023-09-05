
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

        private static readonly Dictionary<SoundName, (string, int)> DicFileNameToLoopCount = new Dictionary<SoundName, (string, int)>();

        /// <summary> Load sound if provided. </summary>
        public static void LoadSound(this IAudio audioService, SoundName soundName, string soundFilePath)
        {
            if (!string.IsNullOrEmpty(soundFilePath) && !DicFileNameToLoopCount.ContainsKey(soundName))
            {
                audioService.Load(soundName, Path.GetFullPath(soundFilePath));
                var loopCount = GetLoopCountForSound(audioService, soundName);
                DicFileNameToLoopCount.Add(soundName, (soundFilePath, loopCount));
            }
        }

        private static int GetLoopCountForSound(IAudio audioService, SoundName soundName)
        {
            var soundLength = audioService.GetLength(soundName);
            var soundLengthMs = (soundLength != TimeSpan.Zero) ? soundLength.TotalMilliseconds : DefaultSoundLengthMs;
            var loopCount = (int)Math.Ceiling(DefaultAlertDurationMs / soundLengthMs);
            return loopCount;
        }

        /// <summary> Plays the sound file provided for specific duration </summary>
        public static void PlaySound(this IAudio audioService, IPropertiesManager properties, SoundName soundName)
        {
            if (!DicFileNameToLoopCount.ContainsKey(soundName))
            {
                int loopCount = DicFileNameToLoopCount[soundName].Item2;

                var alertVolume = properties.GetValue(ApplicationConstants.AlertVolumeKey, DefaultAlertVolume);
                audioService.Play(soundName, loopCount, alertVolume);
            }
        }
    }
}

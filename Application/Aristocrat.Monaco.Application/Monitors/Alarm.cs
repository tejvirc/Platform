namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using Kernel;
    using Hardware.Contracts.Audio;
    using Util;
    using Application.Contracts;

    /// <summary>
    ///     Play audio file as per the requiremnet (error, warning)
    /// </summary>
    public class Alarm
    {
        private readonly IPropertiesManager _properties;
        private readonly IAudio _audioService;
        private string _soundFilePath;

        public Alarm()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IAudio>())
        {
        }

        public Alarm(IPropertiesManager propertiesManager, IAudio audio)
        {
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _audioService = audio ?? throw new ArgumentNullException(nameof(audio));

            _soundFilePath = _properties.GetValue(ApplicationConstants.DefaultAlarmSoundKey, string.Empty);
        }

        /// <summary>
        /// set specific sounf file path, in case user wants to play any sound other than the default sound.
        /// </summary>
        /// <param name="overrideSoundKey">sound key exists in ApplicationConstants file mapped to Apllication.config.xml key</param>
        public void SetAlarmFilePath(string overrideSoundKey)
        {
            if (!string.IsNullOrEmpty(overrideSoundKey))
            {
                _soundFilePath = _properties.GetValue(overrideSoundKey, string.Empty);
            }
        }

        public void LoadAlarm()
        {
            _audioService.LoadSound(_soundFilePath);
        }

        public void PlayAlarm()
        {
            _audioService.PlaySound(_properties, _soundFilePath);
        }
    }
}

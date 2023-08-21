namespace Aristocrat.Monaco.UI.Common.Models
{
    using System;
    using System.Windows.Controls;

    /// <summary>
    /// A setting with string live and edited values.
    /// </summary>
    public class LiveStringSetting : LiveSetting<string>
    {
        private int _minLength, _maxLength;
        private bool _isAlphaNumeric = true;
        private CharacterCasing _characterCasing = CharacterCasing.Normal;

        /// <InheritDoc/>
        public LiveStringSetting(ILiveSettingParent parent, string name)
            : base(parent, name)
        {
        }

        /// <summary>
        /// The setting's minimum length, in characters.
        /// </summary>
        public int MinLength
        {
            get => _minLength;
            set
            {
                _minLength = value;
                RaisePropertyChanged(nameof(MinLength));
            }
        }

        /// <summary>
        /// The setting's maximum length, in characters.
        /// </summary>
        public int MaxLength
        {
            get => _maxLength;
            set
            {
                _maxLength = value;
                RaisePropertyChanged(nameof(MaxLength));
            }
        }

        /// <summary>
        /// Does the setting allow alphanumeric values?
        /// </summary>
        public bool IsAlphaNumeric
        {
            get => _isAlphaNumeric;
            set
            {
                _isAlphaNumeric = value;
                RaisePropertyChanged(nameof(IsAlphaNumeric));
            }
        }

        /// <summary>
        /// What character casing does this setting allow?
        /// </summary>
        public CharacterCasing CharacterCasing
        {
            get => _characterCasing;
            set
            {
                _characterCasing = value;
                RaisePropertyChanged(nameof(CharacterCasing));
            }
        }
    }
}
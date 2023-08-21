namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Contracts;

    /// <summary>
    ///     Defines the AttractInfo class
    /// </summary>
    [CLSCompliant(false)]
    public class AttractInfo : IAttractInfo
    {
        private bool _isSelected;

        /// <inheritdoc />
        public GameType GameType { get; set; }

        /// <inheritdoc />
        public string ThemeId { get; set; }

        /// <inheritdoc />
        public int SequenceNumber { get; set; }
        
        /// <inheritdoc />
        public string ThemeNameDisplayText { get; set; }

        /// <inheritdoc />
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Invoked when the property is changed
        /// </summary>
        /// <param name="propertyName"> Property which changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        /// <inheritdoc />
        public object Clone()
        {
            return new AttractInfo
            {
                GameType = GameType,
                ThemeId = ThemeId,
                ThemeNameDisplayText = ThemeNameDisplayText,
                IsSelected = IsSelected,
                SequenceNumber = SequenceNumber
            };
        }
    }
}
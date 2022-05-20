namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using MVVM.Model;
    using System;

    public class ProgressiveWinModel : BaseNotify
    {
        private DateTime _winDateTime;
        private string _levelName;
        private string _winAmount;
        private int _deviceId;
        private int _levelId;
        private long _transactionId;

        /// <summary>
        ///     Gets or sets the progressive win amount of current game
        /// </summary>
        public string WinAmount
        {
            get => _winAmount;

            set
            {
                _winAmount = value;
                RaisePropertyChanged(nameof(WinAmount));
            }
        }

        /// <summary>
        ///     Gets or sets the progressive win datetime of current game
        /// </summary>
        public DateTime WinDateTime
        {
            get => _winDateTime;

            set
            {
                _winDateTime = value;
                RaisePropertyChanged(nameof(WinDateTime));
            }
        }

        /// <summary>
        ///     Gets or sets the progressive win level name of current game
        /// </summary>
        public string LevelName
        {
            get => _levelName;

            set
            {
                _levelName = value;
                RaisePropertyChanged(nameof(LevelName));
            }
        }

        /// <summary>
        ///     Gets or sets the progressive win level id of current game
        /// </summary>
        public int LevelId
        {
            get => _levelId;

            set
            {
                _levelId = value;
                RaisePropertyChanged(nameof(LevelId));
            }
        }

        /// <summary>
        ///     Gets or sets the progressive win device id of current game
        /// </summary>
        public int DeviceId
        {
            get => _deviceId;

            set
            {
                _deviceId = value;
                RaisePropertyChanged(nameof(DeviceId));
            }
        }

        /// <summary>
        ///     Gets or sets the progressive transaction id of current game
        /// </summary>
        public long TransactionId
        {
            get => _transactionId;

            set
            {
                _transactionId = value;
                RaisePropertyChanged(nameof(TransactionId));
            }
        }
    }
}
namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;
    using Application.Contracts.Extensions;
    using MVVM.ViewModel;

    /// <summary>
    ///     Model for game performance.
    /// </summary>
    [CLSCompliant(false)]
    public class GamePerformanceData : BaseViewModel
    {
        private bool _isActive;

        /// <summary> Enum of possible game combo active states</summary>
        public enum GamePerformanceActiveState
        {
            /// <summary>Game combo is currently active and enabled</summary>
            Active = 0,

            /// <summary>Game combo is not active but some games were played</summary>
            PreviouslyActive = 1,

            /// <summary>Game combo is not active and no games were played</summary>
            NeverActive = 2
        }

        /// <summary> Get the current active state for the game combo</summary>
        public GamePerformanceActiveState ActiveState
        {
            get
            {
                if (IsActive)
                {
                    return GamePerformanceActiveState.Active;
                }

                if (PreviousActiveTime.Ticks > 0)
                {
                    return GamePerformanceActiveState.PreviouslyActive;
                }

                return GamePerformanceActiveState.NeverActive;
            }
        }

        /// <summary>
        ///     Gets the Number and name of the game.
        /// </summary>
        public string ThemeId => GameNumber + " - " + GameName;

        /// <summary>
        ///     Maximum length of any available game's Game Number, used for sorting
        /// </summary>
        public int MaxGameNumberLength { get; set; } = 10;

        /// <summary>
        ///     Gets the name first and then number of the game with leading zeroes for sorting purposes
        /// </summary>
        public string SortableGameName => GameName + GameNumber.ToString().PadLeft(MaxGameNumberLength, '0');

        /// <summary>
        ///     Gets the average bet.
        /// </summary>
        public decimal AverageBet => GamesPlayed == 0 ? 0 : AmountIn / GamesPlayed;

        /// <summary>
        ///     Gets or sets the paytable name.
        /// </summary>
        public string PaytableId { get; set; }

        /// <summary> Gets or sets the denom in cents.</summary>
        public decimal Denomination { get; set; }

        /// <summary> Gets or sets the credits in.</summary>
        public decimal AmountIn => AmountInMillicents.MillicentsToDollars();

        /// <summary> Gets or sets the credits out.</summary>
        public decimal AmountOut => AmountOutMillicents.MillicentsToDollars();

        /// <summary> Gets or sets the number of games played.</summary>
        public long GamesPlayed { get; set; }

        /// <summary> Gets or sets if this game combo is active.</summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value, nameof(ActiveState));
        }

        /// <summary> Gets the days this game has been active.</summary>
        public string DaysActive
        {
            get
            {
                var ts = PreviousActiveTime;
                if (IsActive)
                {
                    if (DateTime.UtcNow > ActiveDateTime)
                    {
                        ts += DateTime.UtcNow - ActiveDateTime;
                    }
                }

                return Convert.ToInt64(ts.TotalDays).ToString();
            }
        }

        /// <summary> Gets the Theoretical RTP for this game.</summary>
        public Tuple<decimal, decimal> TheoreticalRtp { get; set; }

        /// <summary> Gets the actual RTP for this game.</summary>
        public decimal ActualRtp
        {
            get
            {
                if (AmountInMillicents == 0)
                {
                    return 100.0M;
                }

                return 100.0M * AmountOutMillicents / AmountInMillicents;
            }
        }

        /// <summary> Gets the Actual Hold for this game.</summary>
        public decimal ActualHold => 100.0M - ActualRtp;

        /// <summary> Gets or sets the amount of time this game was previously active.</summary>
        public TimeSpan PreviousActiveTime { get; set; }

        /// <summary> Gets or sets the date this game was installed.</summary>
        public DateTime ActiveDateTime { get; set; }

        /// <summary> Gets or sets the credits in (in millicents).</summary>
        public long AmountInMillicents { get; set; }

        /// <summary> Gets or sets the credits out (in millicents).</summary>
        public long AmountOutMillicents { get; set; }

        /// <summary> Gets or sets the GameType of this GamePerformanceData.</summary>
        public GameType GameType { get; set; }

        /// <summary> Gets or sets the GameType of this GamePerformanceData.</summary>
        public string GameSubtype { get; set; }

        /// <summary> Gets or sets the name of the game</summary>
        public string GameName { get; set; }

        /// <summary> Gets or sets the unique id of the game+variation</summary>
        public int GameId { get; set; }

        /// <summary> Gets or sets the unique id of the game+variation+denom</summary>
        public long GameNumber { get; set; }
    }
}
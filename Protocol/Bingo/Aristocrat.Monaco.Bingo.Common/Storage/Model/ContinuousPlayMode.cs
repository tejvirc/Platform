namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.ComponentModel;

    public enum ContinuousPlayMode
    {
        /// <summary>
        ///     Mode used when we allow one press and holding the button to begin game play.
        /// </summary>
        [Description("Play Button One Press")]
        PlayButtonOnePress,

        /// <summary>
        ///     An unsupported mode that causes a lockup when received from the server.
        /// </summary>
        [Description("Play Button Three Press")]
        PlayButtonThreePress,

        /// <summary>
        ///     An unsupported mode that causes a lockup when received from the server.
        /// </summary>
        [Description("Play Button Active Participation")]
        PlayButtonActiveParticipation,

        /// <summary>
        ///     Mode used when we allow one press, but not holding, of the button to begin game play.
        /// </summary>
        [Description("Play Button One Press No Repeat")]
        PlayButtonOnePressNoRepeat,

        /// <summary>
        ///     An unsupported mode that causes a lockup when received from the server.
        /// </summary>
        [Description("Play Button Two Touch")]
        PlayButtonTwoTouch,

        /// <summary>
        ///     An unsupported mode that causes a lockup when received from the server.
        /// </summary>
        [Description("Play Button Two Touch Repeat")]
        PlayButtonTwoTouchRepeat,

        /// <summary>
        ///     The maximum value for ContinuousPlayMode
        ///     This strategy should never be used
        /// </summary>
        [Description("Unknown")]
        Unknown = 99
    }
}

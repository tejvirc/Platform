namespace Aristocrat.Monaco.Gaming
{
    using Contracts;
    using Contracts.Progressives;
    using Kernel;
    using Progressives;
    using RtpRange = Contracts.Rtp.RtpRange;

    /// <summary>
    ///     A set of <see cref="IGameDetail" /> extensions
    /// </summary>
    public static class GameDetailExtensions
    {
        /// <summary>
        ///     Gets the total jurisdictional rtp range
        /// </summary>
        /// <param name="this">The game detail</param>
        /// <param name="denom">The denomination</param>
        public static (RtpRange Rtp, RtpVerifiedState VerifiedState) GetTotalJurisdictionRtpRange(this IGameDetail @this, long denom)
        {
            if (@this == null)
            {
                return (new RtpRange(0M, 0M), RtpVerifiedState.NotAvailable);
            }

            var progressiveConfigurationProvider = ServiceManager.GetInstance()
                .GetService<IContainerService>().Container
                .GetInstance<IProgressiveConfigurationProvider>();

            return @this.GetTotalJurisdictionRtpRange(
                progressiveConfigurationProvider.GetProgressivePackRtp(
                    @this.Id,
                    denom,
                    @this.GetBetOption(denom)?.Name));
        }

        /// <summary>
        ///     Gets the total jurisdictional rtp range
        /// </summary>
        /// <param name="this">The game detail</param>
        /// <param name="data">The progressive (rtp, state) for game</param>
        public static (RtpRange, RtpVerifiedState) GetTotalJurisdictionRtpRange(
            this IGameDetail @this,
            (ProgressiveRtp progressiveRtp, RtpVerifiedState progressiveRtpState) data)
        {
            if (@this == null)
            {
                return (new RtpRange(0M, 0M), RtpVerifiedState.NotAvailable);
            }

            if (data.progressiveRtp is null || data.progressiveRtpState != RtpVerifiedState.Verified)
            {
                return (@this.GetBaseGameRtpRange(), data.progressiveRtpState);
            }

            var includeIncrementRtp = ServiceManager.GetInstance().GetService<IContainerService>().Container
                .GetInstance<IGameProvider>()
                .CanIncludeIncrementRtp(@this.GameType);

            return (includeIncrementRtp
                    ? data.progressiveRtp.BaseAndResetAndIncrement
                    : data.progressiveRtp.BaseAndReset, RtpVerifiedState.Verified);
        }

        /// <summary>
        ///     Gets the game's base game rtp range
        /// </summary>
        /// <param name="this">The game detail</param>
        public static RtpRange GetBaseGameRtpRange(this IGameDetail @this)
        {
            if (@this is null)
            {
                return new RtpRange(0M, 0M);
            }

            return new RtpRange(@this.MinimumPaybackPercent, @this.MaximumPaybackPercent);
        }
    }
}
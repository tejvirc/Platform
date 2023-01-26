namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    /// <summary>
    ///     
    /// </summary>
    public interface IRtpService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public GameRtpReport GenerateRtpReportForGame(IGameProfile game);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameThemeId"></param>
        /// <returns></returns>
        public GameRtpReport GenerateRtpReportForGame(string gameThemeId);

    }
}
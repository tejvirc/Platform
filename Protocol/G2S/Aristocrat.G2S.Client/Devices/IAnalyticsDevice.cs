namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with the Analytics device
    /// </summary>
    public interface IAnalyticsDevice : IDevice, ISingleDevice, IRestartStatus
    {
        /// <summary>
        ///     Sends the track command and responds with trackAck
        /// </summary>
        /// <param name="command">The track command</param>
        /// <returns>The trackAck command</returns>
        public trackAck SendTrack(track command);
    }
}

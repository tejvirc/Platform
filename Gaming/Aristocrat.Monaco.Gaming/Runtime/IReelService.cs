namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System.Collections.Generic;
    using Client;
    using Hardware.Contracts.Reel;

    /// <summary>
    ///     Provides a mechanism to communicate reel information with a runtime client
    /// </summary>
    public interface IReelService : IClientEndpoint
    {
        void UpdateReelState(IDictionary<int, ReelLogicalState> updateData);
    }
}
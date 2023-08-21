namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System.Collections.Generic;
    using Common.Storage.Model;
    using ServerApiGateway;

    /// <summary>
    ///     Generalizes behavior for all configuration types sent by the Bingo Server
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        ///     Handles each setting based on provided attribute type.
        /// </summary>
        /// <param name="messageConfigurationAttribute">The attribute for one of the configuration types</param>
        /// <param name="model">The model used to saving and loading server settings</param>
        void Configure(IEnumerable<ConfigurationResponse.Types.ClientAttribute> messageConfigurationAttribute, BingoServerSettingsModel model);
    }
}

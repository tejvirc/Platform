namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to save and retrieve the configured attract sequence.
    /// </summary>
    public interface IAttractConfigurationProvider
    {
        /// <summary>
        ///  Gets the attract sequence
        /// </summary>
        /// <returns></returns>
        IEnumerable<IAttractInfo> GetAttractSequence();

        /// <summary>
        ///  Saves the configured attract sequence
        /// </summary>
        /// <param name="attractSequence"></param>
        void SaveAttractSequence(ICollection<IAttractInfo> attractSequence);

        /// <summary>
        /// To check whether attract is enabled or not
        /// </summary>
        bool IsAttractEnabled { get; }

        /// <summary>
        /// Gets the default attract sequence
        /// </summary>
        /// <returns>Default attract sequence</returns>
        IEnumerable<IAttractInfo> GetDefaultSequence();
    }
}
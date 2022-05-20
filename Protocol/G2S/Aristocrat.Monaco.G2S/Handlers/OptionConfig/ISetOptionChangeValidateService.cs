namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Protocol.v21;
    using Monaco.Common.Validation;

    /// <summary>
    ///     Interface for validator of option config change entries.
    /// </summary>
    public interface ISetOptionChangeValidateService
    {
        /// <summary>
        ///     Validates values in 'setOptionChange' command.
        /// </summary>
        /// <param name="command">Command instance.</param>
        /// <returns>Test</returns>
        ValidationResult Verify(ClassCommand<optionConfig, setOptionChange> command);
    }
}
namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     An interface through which a help text can be obtained 
    /// </summary>
    public interface IGameHelpTextProvider
    {
        /// <summary>Gets the collection of help text builder</summary>
        IDictionary<string, Func<string>> AllHelpTexts { get; }
    }
}

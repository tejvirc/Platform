namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using System;
    using System.Collections.Generic;
    using Gaming.Contracts;

    /// <summary>
    ///     All display configuration data for Bingo.
    /// </summary>
    [Serializable]
    public class BingoDisplayConfiguration
    {
        /// <summary>
        ///     Bingo Info settings for each window.
        /// </summary>
        public List<BingoWindowSettings> BingoInfoWindowSettings { get; set; }

        /// <summary>
        ///     Bingo help appearance.
        /// </summary>
        public BingoHelpAppearance HelpAppearance { get; set; }

        /// <summary>
        ///     Bingo attract settings
        /// </summary>
        public BingoAttractSettings BingoAttractSettings { get; set; }

        /// <summary>
        ///     Gets or sets the presentation override message formats
        /// </summary>
        public List<SerializableKeyValuePair<PresentationOverrideTypes, string>> PresentationOverrideMessageFormats { get; set; }
    }

    /// <summary>
    ///     A serializable key value pair
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    [Serializable]
    public struct SerializableKeyValuePair<TKey, TValue>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableKeyValuePair{TKey, TValue}" /> structure
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        ///     Gets or sets the key
        /// </summary>
        public TKey Key { get; set; }
        
        /// <summary>
        ///     Gets or sets the value
        /// </summary>
        public TValue Value { get; set; }
    }
}

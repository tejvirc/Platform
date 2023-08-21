namespace Aristocrat.Monaco.UI.Common.Controls.Helpers
{
    using System;
    using System.Windows.Input;

    /// <summary>
    ///     Extenstion methods for Key
    /// </summary>
    public static class KeyExtensions
    {
        /// <summary>
        ///     Gets whether or not a key is numeric or not
        /// </summary>
        /// <param name="key">The key to check</param>
        public static bool IsNumeric(this Key key)
        {
            return (key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9);
        }

        /// <summary>
        ///     Gets whether or not a key is alphabetic or not
        /// </summary>
        /// <param name="key">The key to check</param>
        public static bool IsAlphabetic(this Key key)
        {
            return key >= Key.A && key <= Key.Z;
        }

        /// <summary>
        ///     Gets whether or not a key is alphanumeric or not
        /// </summary>
        /// <param name="key">The key to check</param>
        public static bool IsAlphaNumeric(this Key key)
        {
            return key.IsAlphabetic() || key.IsNumeric();
        }

        /// <summary>
        ///     Gets whether or not a key is a modifier (delete or backspace key)
        /// </summary>
        /// <param name="key">The key to check</param>
        public static bool IsModifier(this Key key)
        {
            return key == Key.Back || key == Key.Delete || key == Key.Tab;
        }

        /// <summary>
        ///     Gets the numerical value for the provided key or throws ArgumentException if the key is not numeric
        /// </summary>
        /// <param name="key">The key to get a numeric value for</param>
        public static int GetDigitFromKey(this Key key)
        {
            switch (key)
            {
                case Key.D0:
                case Key.NumPad0: return 0;
                case Key.D1:
                case Key.NumPad1: return 1;
                case Key.D2:
                case Key.NumPad2: return 2;
                case Key.D3:
                case Key.NumPad3: return 3;
                case Key.D4:
                case Key.NumPad4: return 4;
                case Key.D5:
                case Key.NumPad5: return 5;
                case Key.D6:
                case Key.NumPad6: return 6;
                case Key.D7:
                case Key.NumPad7: return 7;
                case Key.D8:
                case Key.NumPad8: return 8;
                case Key.D9:
                case Key.NumPad9: return 9;
                default: throw new ArgumentException("Invalid key: " + key);
            }
        }
    }
}
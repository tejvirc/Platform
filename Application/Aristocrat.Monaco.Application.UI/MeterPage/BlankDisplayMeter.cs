namespace Aristocrat.Monaco.Application.UI.MeterPage
{
    using System;

    /// <summary>
    ///     This is for creating blank lines in a list of meters in the operator menu. 
    ///     This is created by the meters page viewmodel implementation for use in meters page UIs.
    ///     To create a blank line, in the meter xml set displayName to ""
    ///     and set the meter name to "blank line" in the meter xml
    /// </summary>
    [CLSCompliant(false)]
    public class BlankDisplayMeter : DisplayMeter
    {
        /// <summary>
        ///     Creates an instance of the class
        /// </summary>
        /// <param name="showLifetime">true to show Lifetime, false to show Period</param>
        /// <param name="order">the order of the blank line relative to the other meters</param>
        public BlankDisplayMeter(bool showLifetime, int order)
            :base(string.Empty, null, showLifetime, order)
        {
        }

        /// <inheritdoc />
        public override string Value => string.Empty;
    }
}
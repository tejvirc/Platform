namespace Aristocrat.Monaco.Accounting.UI.Models
{
    using Kernel.MarketConfig.Models.Application;

    /// <summary>
    ///     Defines the bar code types
    /// </summary>
    public class BarcodeTypeData
    {
        /// <summary>
        /// </summary>
        /// <param name="value"> The (enum) value for bar code type</param>
        /// <param name="description"> The description for corresponding bar code type</param>
        public BarcodeTypeData(BarcodeTypeOptions value, string description)
        {
            Value = value;
            Description = description;
        }

        /// <summary>
        ///     Gets the Value for bar code type
        /// </summary>
        public BarcodeTypeOptions Value { get; }

        /// <summary>
        ///     Gets the Description for bar code type
        /// </summary>
        public string Description { get; }
    }
}

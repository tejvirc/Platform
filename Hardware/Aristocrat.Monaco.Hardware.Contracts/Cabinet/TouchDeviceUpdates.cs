namespace Aristocrat.Monaco.Hardware.Contracts.Cabinet
{
    /// <summary>
    ///     Updates for touch devices
    /// </summary>
    public ref struct TouchDeviceUpdates
    {
        /// <summary>
        ///     Creates an instance of <see cref="TouchDeviceUpdates"/>
        /// </summary>
        /// <param name="model">The model to use for the update</param>
        /// <param name="product">The product used for the update</param>
        /// <param name="versionNumber">The version number used for the update</param>
        public TouchDeviceUpdates(string model, string product, int versionNumber)
        {
            Model = model;
            Product = product;
            VersionNumber = versionNumber;
        }

        /// <summary>
        ///     Gets the model for the touch device to update
        /// </summary>
        public string Model { get; }

        /// <summary>
        ///     Gets the product for the touch device to update
        /// </summary>
        public string Product { get; }

        /// <summary>
        ///     Gets the version for the touch device to update
        /// </summary>
        public int VersionNumber { get; }
    }
}
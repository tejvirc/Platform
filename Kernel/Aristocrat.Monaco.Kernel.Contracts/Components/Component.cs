namespace Aristocrat.Monaco.Kernel.Contracts.Components
{
    using System;

    /// <summary>
    ///     Component entity defines a software module (component).
    /// </summary>
    public class Component
    {
        /// <summary>
        ///     Gets or sets component identifier
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        ///     Gets or sets component type
        /// </summary>
        public ComponentType Type { get; set; }

        /// <summary>
        ///     Gets or sets description of the component
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets number of bytes for the component
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating the directory path or file path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the directory path or file path or volume.
        /// </summary>
        public FileSystemType FileSystemType { get; set; }

        /// <summary>
        ///     Gets or sets whether the component is available
        /// </summary>
        public bool Available { get; set; } = true;
        
        /// <summary>
        ///     Gets or sets whether the component has a fault
        /// </summary>
        public bool HasFault { get; set; }

        /// <summary>
        ///     Handles notification when component becomes available
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">event arguments</param>
        public void OnAvailable(object sender, EventArgs e)
        {
            Available = true;
        }

        /// <summary>
        ///     Handles notification when component becomes unavailable
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">event arguments</param>
        public void OnUnavailable(object sender, EventArgs e)
        {
            Available = false;
        }

        /// <summary>
        ///     Handles notification when a component fault occurs
        /// </summary>
        public void OnFaultOccurred()
        {
            HasFault = true;
        }

        /// <summary>
        ///     Handles notification when all component faults are cleared
        /// </summary>
        public void OnAllFaultsCleared()
        {
            HasFault = false;
        }
    }
}
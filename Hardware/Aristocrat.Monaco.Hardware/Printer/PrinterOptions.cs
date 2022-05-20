using System;

namespace Aristocrat.Monaco.Hardware.Printer
{
    /// <summary>A printer reader options object.</summary>
    public struct PrinterOptions
    {
        /// <summary>Gets or sets the render target.</summary>
        /// <value>The render target.</value>
        public string RenderTarget { get; set; }

        /// <summary>Gets or sets the activation time.</summary>
        /// <value>The last activation time.</value>
        public DateTime ActivationTime { get; set; }
    }
}
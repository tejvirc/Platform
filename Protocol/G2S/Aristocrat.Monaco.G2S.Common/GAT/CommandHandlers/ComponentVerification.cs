namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;

    /// <summary>
    ///     Component verification model
    /// </summary>
    public class ComponentVerification
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentVerification" /> class.
        /// </summary>
        /// <param name="componentId">Component identifier</param>
        /// <param name="passed">Parameter that defines component verification state passed or failed</param>
        public ComponentVerification(string componentId, bool passed)
        {
            if (string.IsNullOrEmpty(componentId))
            {
                throw new ArgumentNullException(nameof(componentId));
            }

            ComponentId = componentId;
            Passed = passed;
        }

        /// <summary>
        ///     Gets component identifier
        /// </summary>
        public string ComponentId { get; }

        /// <summary>
        ///     Gets a value indicating whether gets parameter that defines component verification state passed or fail
        /// </summary>
        public bool Passed { get; }
    }
}
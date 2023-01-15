namespace Aristocrat.Monaco.Hhr.Client.WorkFlow
{
    using System;
    using Kernel.Contracts.MessageDisplay;
    using Messages;

    /// <summary>
    ///     Defines attributes for timeout behavior where lockup needs to be created.
    /// </summary>
    public class LockupRequestTimeout : IRequestTimeout
    {
        /// <summary>
        /// </summary>
        public LockupRequestTimeout()
        {
            TimeoutBehaviorType = TimeoutBehaviorType.Lockup;
        }

        /// <summary>
        ///     Lockup Guid to create.
        /// </summary>
        public Guid LockupKey { get; set; }

        /// <summary>
        ///     Lockup string to be displayed on screen for this lockup.
        /// </summary>
        //public string LockupString { get; set; }
        public string LockupStringResouceKey { get; set; }

        /// <summary>
        ///     Culture provider to load Lockup text from resources
        /// </summary>
        public CultureProviderType ProviderType { get; set; }

        /// <summary>
        ///     Help text for this lockup.
        /// </summary>
        public string LockupHelpText { get; set; }

        /// <inheritdoc />
        public TimeoutBehaviorType TimeoutBehaviorType { get; }
    }
}
namespace Aristocrat.Monaco.Sas.Consumers
{
    /// <summary>An interface through which some system events can be handled.</summary>
    public interface ISystemEventHandler
    {
        /// <summary>Called when SAS is started.</summary>
        /// <param name="isFirstLoad">Whether or not this is a first load of the platform</param>
        void OnPlatformBooted(bool isFirstLoad);

        /// <summary>Called when Sas has started</summary>
        void OnSasStarted();

        /// <summary>Called when the system is enabled.</summary>
        void OnSystemEnabled();

        /// <summary>Called when the system is disabled.</summary>
        void OnSystemDisabled();
    }
}

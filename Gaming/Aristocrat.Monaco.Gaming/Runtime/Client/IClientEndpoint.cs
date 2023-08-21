namespace Aristocrat.Monaco.Gaming.Runtime.Client
{
    /// <summary>
    ///     Provides a mechanism to communicate with a runtime client
    /// </summary>
    public interface IClientEndpoint
    {
        bool Connected { get; }
    }
}
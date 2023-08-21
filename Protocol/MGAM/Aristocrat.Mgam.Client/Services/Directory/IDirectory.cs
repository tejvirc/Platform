namespace Aristocrat.Mgam.Client.Services.Directory
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     Manages communication with the Directory service.
    /// </summary>
    public interface IDirectory : IHostService
    {
        /// <summary>
        ///     Locates services on the site-controller.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="listener"></param>
        /// <returns><see cref="Task"/>.</returns>
        Task<IDisposable> LocateServices(string serviceName, Func<IPEndPoint, Task> listener);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="deviceName">Device Name.</param>
        /// <param name="manufacturerName">Manufacturer Name.</param>
        /// <param name="listener">Lister.</param>
        /// <returns><see cref="Task"/>.</returns>
        Task<IDisposable> LocateXadf(
            string deviceName,
            string manufacturerName,
            Func<RequestXadfResponse, Task> listener);

        /// <summary>
        ///     Obtains valid GUID from the host.
        /// </summary>
        /// <returns><see cref="Task"/>.</returns>
        Task<Guid> NewGuid(CancellationToken cancellationToken = default);
    }
}

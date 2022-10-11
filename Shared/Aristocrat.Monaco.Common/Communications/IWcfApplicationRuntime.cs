namespace Aristocrat.Monaco.Common.Communications
{
    using System;
    using System.Threading.Tasks;
    using CoreWCF;
    using CoreWCF.Configuration;

    /// <summary>
    /// 
    /// </summary>
    public interface IWcfApplicationRuntime
    {
        /// <summary>
        /// 
        /// </summary>
        CommunicationState State { get; }

        /// <summary>
        /// 
        /// </summary>
        void Start();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        void UseServiceModel(Action<IServiceBuilder> builder);

        /// <summary>
        /// 
        /// </summary>
        ValueTask DisposeAsync();

        /// <summary>
        /// 
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetRequiredService<T>() where T : notnull;
    }
}

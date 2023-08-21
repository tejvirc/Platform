namespace Aristocrat.Monaco.Mgam.Services.GameConfiguration
{
    using System.Threading.Tasks;

    public interface IGameConfigurator
    {
        /// <summary>
        ///     Configure games based on operator host configurations.
        /// </summary>
        /// <returns></returns>
        Task Configure();
    }
}

namespace Aristocrat.G2S.Client
{
    /// <summary>
    ///     Provides a mechanism to define a G2S EGM.
    /// </summary>
    public interface IEgm
    {
        /// <summary>
        ///     Gets the unique Electronic Gaming Machine (EGM) Identifier
        /// </summary>
        string Id { get; }
    }
}
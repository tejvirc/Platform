namespace Aristocrat.G2S.Emdi.Extensions
{
    using Monaco.Application.Contracts.Media;
    using System.Linq;

    /// <summary>
    /// Extension methods for media classes
    /// </summary>
    internal static class MediaExtensions
    {
        public static bool PlayerExist(this IMediaProvider media, int port)
        {
            return media.GetMediaPlayers().Any(p => p.Port == port);
        }
    }
}

namespace Aristocrat.Monaco.Gaming.Lobby.Services.Attract;

using System.Threading.Tasks;

public interface IAttractService
{
    void SetLanguageFlags(int currAttractIndex);

    int AdvanceAttractIndex();

    AttractVideoPathsResult SetAttractVideoPaths(int currAttractIndex);

    bool PlayAdditionalConsecutiveVideo();

    void NotifyEntered();

    void ResetConsecutiveAttractCount();
}

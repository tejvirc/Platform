namespace Aristocrat.Monaco.Gaming.Lobby.Services.Attract;

using System.Threading.Tasks;

public interface IAttractService
{
    void SetAttractVideoPaths(int currAttractIndex);

    void NotifyEntered();

    void RotateTopImage();

    void RotateTopperImage();
}

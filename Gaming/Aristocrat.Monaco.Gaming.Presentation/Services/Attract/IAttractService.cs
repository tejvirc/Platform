namespace Aristocrat.Monaco.Gaming.Presentation.Services.Attract;

using System.Threading.Tasks;

public interface IAttractService
{
    void SetAttractVideoPaths(int currAttractIndex);

    void NotifyEntered();
}

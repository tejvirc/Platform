namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System.Threading.Tasks;

public interface ILayoutManager
{
    Task InitializeAsync();

    Task ShutdownAsync();
}

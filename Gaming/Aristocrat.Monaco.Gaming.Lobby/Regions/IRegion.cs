namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRegion : IEnumerable<object>
{
    string Name { get; }

    IReadOnlyCollection<object> Views { get; }

    IReadOnlyCollection<object> ActiveViews { get; }

    Task ActivateViewAsync(object view);

    Task ActivateViewAsync(string viewName);

    Task DeactivateViewAsync(object view);

    Task DeactivateViewAsync(string viewName);
    Task<object> GetViewAsync(string viewName);

    Task AddViewAsync(string viewName, object view);

    Task RemoveViewAsync(string viewName);

    Task RemoveViewAsync(object view);

    Task ClearViewsAsync();

    Task<bool> NavigateToAsync(object view);

    Task<bool> NavigateToAsync(string viewName);
}

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
    object GetView(string viewName);

    void AddView(string viewName, object view);

    void RemoveView(string viewName);

    void RemoveView(object view);

    void ClearViews();

    Task<bool> NavigateToAsync(object view);

    Task<bool> NavigateToAsync(string viewName);
}

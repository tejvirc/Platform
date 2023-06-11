namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Collections.ObjectModel;

public interface IRegion
{
    string Name { get; }

    ViewCollection Views { get; }

    ViewCollection ActiveViews { get; }

    void ActivateView(object view);

    void ActivateView(string viewName);

    void DeactivateView(object view);

    void DeactivateView(string viewName);

    object GetView(string viewName);

    void AddView(string viewName, object view);

    void RemoveView(string viewName);

    void RemoveView(object view);

    void ClearViews();

    bool NavigateTo(object view);

    bool NavigateTo(string viewName);
}

namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Collections.ObjectModel;
using System.Linq;

public abstract class Region : IRegion
{
    private readonly IRegionManager _regionManager;
    private readonly IRegionNavigator _regionNavigator;

    private readonly ObservableCollection<ViewItem> _items = new();

    private ViewCollection? _views;
    private ViewCollection? _activeViews;

    protected Region(IRegionManager regionManager, IRegionNavigator regionNavigator, string regionName)
    {
        _regionManager = regionManager;
        _regionNavigator = regionNavigator;
        Name = regionName;
    }

    public string Name { get; init; }

    public ViewCollection Views => _views ??= new ViewCollection(_items, _ => true);

    public ViewCollection ActiveViews => _activeViews ??= new ViewCollection(_items, x => x.IsActive);

    public virtual void ActivateView(object view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        var item = _items.First(x => ReferenceEquals(x.View, view));

        if (!item.IsActive)
        {
            item.IsActive = true;
        }
    }

    public void ActivateView(string viewName) => ActivateView(GetView(viewName));

    public virtual void DeactivateView(object view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        var viewItem = _items.FirstOrDefault(x => ReferenceEquals(x.View, view));
        if (viewItem == null)
        {
            throw new ArgumentOutOfRangeException(nameof(view));
        }

        if (viewItem.IsActive)
        {
            viewItem.IsActive = false;
        }
    }

    public void DeactivateView(string viewName) => DeactivateView(GetView(viewName));

    public object GetView(string viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
        {
            throw new ArgumentNullException(nameof(viewName));
        }

        return _items.Where(x => x.ViewName == viewName).Select(x => x.View).First();
    }

    public void AddView(string viewName, object view)
    {
        if (_items.Any(x => x.ViewName == viewName || ReferenceEquals(x.View, view)))
        {
            throw new ArgumentException($@"Region already contains {viewName} view", nameof(viewName));
        }

        _items.Add(new ViewItem(viewName, view));
    }

    public void RemoveView(string viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
        {
            throw new ArgumentNullException(nameof(viewName));
        }

        var item =  _items.First(x => x.ViewName == viewName);

        RemoveView(item);
    }

    public void RemoveView(object view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        var item = _items.First(x => ReferenceEquals(x.View, view));

        RemoveView(item);
    }

    private void RemoveView(ViewItem item)
    {
        _items.Remove(item);
    }

    public void ClearViews()
    {
        _items.Clear();
    }

    public bool NavigateTo(object view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        var item = _items.First(x => ReferenceEquals(x.View, view));

        return NavigateTo(item);
    }

    public bool NavigateTo(string viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
        {
            throw new ArgumentNullException(nameof(viewName));
        }

        var item = _items.First(x => x.ViewName == viewName);

        return NavigateTo(item);
    }

    private bool NavigateTo(ViewItem item)
    {
        ActivateView(item.View);

        return true;
        // return _regionNavigator.NavigateTo(item.ViewName);
    }
}

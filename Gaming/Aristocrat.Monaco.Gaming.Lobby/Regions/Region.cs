namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Navigation;

public class Region : IRegion
{
    private readonly IRegionManager _regionManager;
    private readonly IRegionNavigator _regionNavigator;

    private readonly Dictionary<string, RegionView> _views = new();

    public Region(IRegionManager regionManager, IRegionNavigator regionNavigator)
    {
        _regionManager = regionManager;
        _regionNavigator = regionNavigator;
    }

    public string Name { get; init; }

    public IReadOnlyCollection<object> Views => _views.Values.Select(x => x.View).ToList();

    public IReadOnlyCollection<object> ActiveViews => _views.Values.Where(x => x.IsActive).Select(x => x.View).ToList();

    public async Task ActivateViewAsync(object view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        var viewName = _views.Where(x => ReferenceEquals(x.Value.View, view)).Select(x => x.Key).FirstOrDefault();
        if (viewName == null)
        {
            throw new ArgumentOutOfRangeException(nameof(view));
        }

        await ActivateViewAsync(viewName);
    }

    public Task ActivateViewAsync(string viewName)
    {
        if (!_views.TryGetValue(viewName, out var item))
        {
            throw new ArgumentOutOfRangeException(nameof(viewName));
        }

        item.IsActive = true;

        return Task.CompletedTask;
    }

    public async Task DeactivateViewAsync(object view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        var viewName = _views.Where(x => ReferenceEquals(x.Value.View, view)).Select(x => x.Key).FirstOrDefault();
        if (viewName == null)
        {
            throw new ArgumentOutOfRangeException(nameof(view));
        }

        await DeactivateViewAsync(viewName);
    }

    public Task DeactivateViewAsync(string viewName)
    {
        if (!_views.TryGetValue(viewName, out var item))
        {
            throw new ArgumentOutOfRangeException(nameof(viewName));
        }

        item.IsActive = false;

        return Task.CompletedTask;
    }

    public object GetView(string viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
        {
            throw new ArgumentNullException(nameof(viewName));
        }

        if (!_views.TryGetValue(viewName, out var view))
        {
            throw new ArgumentOutOfRangeException(nameof(viewName));
        }

        return view;
    }

    public void AddView(string viewName, object view)
    {
        if (_views.ContainsKey(viewName))
        {
            throw new ArgumentException($"Region already contains {viewName} view", nameof(viewName));
        }

        _views.Add(viewName, new RegionView { Name = viewName, View = view });
    }

    public void RemoveView(string viewName)
    {
        if (!_views.ContainsKey(viewName))
        {
            throw new ArgumentOutOfRangeException(nameof(viewName));
        }

        _views.Remove(viewName);
    }

    public void RemoveView(object view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        var viewName = _views.Where(x => ReferenceEquals(x.Value.View, view)).Select(x => x.Key).FirstOrDefault();
        if (viewName == null)
        {
            throw new ArgumentOutOfRangeException(nameof(view));
        }

        _views.Remove(viewName);
    }

    public void ClearViews()
    {
        _views.Clear();
    }

    public async Task<bool> NavigateToAsync(object view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        var viewName = _views.Where(x => ReferenceEquals(x.Value.View, view)).Select(x => x.Key).FirstOrDefault();
        if (viewName == null)
        {
            throw new ArgumentOutOfRangeException(nameof(view));
        }

        return await NavigateToAsync(viewName);
    }

    public async Task<bool> NavigateToAsync(string viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
        {
            throw new ArgumentNullException(nameof(viewName));
        }

        if (_views.TryGetValue(viewName, out var view))
        {
            throw new ArgumentOutOfRangeException(nameof(viewName));
        }

        return await _regionNavigator.NavigateToAsync(viewName);
    }

    public IEnumerator<object> GetEnumerator()
    {
        return _views.Values.Select(x => x.View).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

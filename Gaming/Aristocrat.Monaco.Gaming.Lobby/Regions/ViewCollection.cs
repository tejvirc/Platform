namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

public class ViewCollection : IEnumerable<object>, INotifyCollectionChanged
{
    private readonly ObservableCollection<ViewItem> _views;
    private readonly Predicate<ViewItem> _filter;

    private List<object> _filteredViews = new();

    public ViewCollection(ObservableCollection<ViewItem> views, Predicate<ViewItem> filter)
    {
        _filter = filter;
        _views = views;

        UpdateFilteredViews();

        _views.CollectionChanged += OnUnfilteredCollectionChanged;
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public IEnumerator<object> GetEnumerator() => _filteredViews.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void UpdateFilteredViews()
    {
        _filteredViews = _views.Where(x => _filter.Invoke(x)).Select(x => x.View).ToList();
    }

    private void OnUnfilteredCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                UpdateFilteredViews();
                var views = e.NewItems?.Cast<ViewItem>().Where(x => _filter(x)).ToList();
                if (views != null)
                {
                    foreach (var view in views)
                    {
                        OnAdded(view);
                    }
                }

                break;
            }

            case NotifyCollectionChangedAction.Remove:
            {
                var items = e.OldItems?.Cast<ViewItem>().Where(x => _filter(x));
                if (items != null)
                {
                    OnRemoved(items);
                }

                break;
            }
        }
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, object[] views, int index)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, views, index));
    }

    private void OnAdded(object view)
    {
        var index = _filteredViews.IndexOf(view);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, new[] { view }, index);
    }

    private void OnRemoved(object view)
    {
        var index = _filteredViews.IndexOf(view);
        UpdateFilteredViews();
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, new[] { view }, index);
    }
}

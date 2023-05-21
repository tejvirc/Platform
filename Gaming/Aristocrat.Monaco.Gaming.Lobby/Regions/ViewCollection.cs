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

                var items = e.NewItems?.Cast<ViewItem>().ToList();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        item.ItemChanged += OnItemChanged;
                        OnAdded(item.View);
                    }
                }

                break;
            }

            case NotifyCollectionChangedAction.Remove:
            {
                var items = e.OldItems?.Cast<ViewItem>().Where(x => _filter(x));
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        item.ItemChanged -= OnItemChanged;
                        OnRemoved(item.View);
                    }
                }

                break;
            }
        }
    }

    private void OnItemChanged(object? sender, EventArgs e)
    {
        if (sender is not ViewItem item)
        {
            return;
        }

        var oldIndex = _filteredViews.IndexOf(item.View);

        UpdateFilteredViews();

        var newIndex = _filteredViews.IndexOf(item.View);

        if (oldIndex != -1 && newIndex == -1)
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, new[] { item.View }, oldIndex);
        }
        else if (oldIndex == -1 && newIndex != -1)
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Add, new[] { item.View }, newIndex);
        }
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, object[] views, int index)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, views, index));
    }

    private void OnAdded(object view)
    {
        var index = _filteredViews.IndexOf(view);

        if (index != -1)
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Add, new[] { view }, index);
        }
    }

    private void OnRemoved(object view)
    {
        var index = _filteredViews.IndexOf(view);

        UpdateFilteredViews();

        if (index != -1)
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, new[] { view }, index);
        }
    }
}

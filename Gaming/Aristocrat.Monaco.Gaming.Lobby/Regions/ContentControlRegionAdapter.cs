namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using Monaco.UI.Common;

public class ContentControlRegionAdapter : RegionAdapter<ContentControl>
{
    private readonly IRegionManager _regionManager;
    private readonly IRegionNavigator _regionNavigator;

    public ContentControlRegionAdapter(IRegionManager regionManager, IRegionNavigator regionNavigator)
    {
        _regionManager = regionManager;
        _regionNavigator = regionNavigator;
    }

    protected override IRegion NewRegion(string regionName)
    {
        return new SingleActiveRegion(_regionManager, _regionNavigator, regionName);
    }

    protected override void Adapt(IRegion region, ContentControl element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        var contentIsSet = element.Content != null;
        contentIsSet = contentIsSet || element.HasBinding(ContentControl.ContentProperty);
        if (contentIsSet)
        {
            throw new InvalidOperationException("ContentControl element has content");
        }

        region.ActiveViews.CollectionChanged += delegate
        {
            element.Content = region.ActiveViews.FirstOrDefault();
        };

        region.Views.CollectionChanged +=
            (_, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add && !region.ActiveViews.Any())
                {
                    var view = e.NewItems?[0];
                    if (view == null)
                    {
                        throw new InvalidOperationException("Added view item cannot be null");
                    }

                    region.ActivateView(view);
                }
            };
    }
}

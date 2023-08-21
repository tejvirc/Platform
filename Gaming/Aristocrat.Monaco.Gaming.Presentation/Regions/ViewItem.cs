namespace Aristocrat.Monaco.Gaming.Presentation.Regions;

using System;

public class ViewItem
{
    private bool _isActive;

    public ViewItem(string viewName, object view)
    {
        ViewName = viewName;
        View = view;
    }

    public event EventHandler? ItemChanged;

    public string ViewName { get; }

    public object View { get; }

    public bool IsActive
    {
        get => _isActive;

        set
        {
            if (_isActive == value)
            {
                return;
            }

            _isActive = value;

            OnItemChanged();
        }
    }

    private void OnItemChanged() => ItemChanged?.Invoke(this, EventArgs.Empty);
}

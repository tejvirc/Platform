namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System.Windows;
using Aristocrat.Cabinet.Contracts;

public interface IScreenMapper
{
    ScreenMapResult Map(DisplayRole role, Window window, bool dryRun = false, ScreenMapOptions? options = null);
}

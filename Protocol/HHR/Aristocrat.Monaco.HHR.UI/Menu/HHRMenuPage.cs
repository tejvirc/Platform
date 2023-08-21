namespace Aristocrat.Monaco.Hhr.UI.Menu
{
    using System;
    using System.Windows.Controls;

    /// <summary>
    ///     Use this base UserControl class when creating an HHR Menu WPF page
    /// </summary>
    public abstract class HHRMenuPage : UserControl, IHHRMenuPage
    {
        internal bool Disposed;

         public void Dispose()
         {
             Dispose(true);
             GC.SuppressFinalize(this);
         }

         protected virtual void Dispose(bool disposing)
         {
             if (Disposed)
             {
                 return;
             }

             if (disposing)
             {
                 if (DataContext is IHhrMenuPageViewModel vm)
                 {
                     vm.Dispose();
                 }
                 Resources.MergedDictionaries.Clear();
                 Resources.Clear();
             }

             Disposed = true;
         }
    }
}

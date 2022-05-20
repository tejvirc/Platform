namespace Aristocrat.Monaco.Hhr.UI.Services
{
    using System.Threading.Tasks;
    using Menu;
    using Views;

    /// <summary>
    ///  Interface to load data into the menu
    /// </summary>
    public interface IMenuAccessService
    {
        /// <summary>
        ///     Shows the menu
        /// </summary>
        Task Show(Command command);
        
        /// <summary>
        ///     Hides the menu
        /// </summary>
        void Hide();

        /// <summary>
        ///     Unhides the menu
        /// </summary>
        void Unhide();

        /// <summary>
        ///     Sets the host page view
        /// </summary>
        void SetView(IHhrHostPageView hhrHostPageView);
    }
}
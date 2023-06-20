namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using Application.UI.OperatorMenu;
    using Application.Contracts.OperatorMenu;

    /// <summary>
    ///     ViewModel for CommsMainPageViewModel.
    /// </summary>
    public class CommsMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/CommsMenu";

        public CommsMainPageViewModel(IOperatorMenuPageLoader mainPage) : base(mainPage, MenuExtensionPointPath)
        {
        }
    }
}

namespace Aristocrat.Monaco.Application.UI.ButtonTestDeck
{
    using ViewModels;
    using Views;

    public class LCDButtonTestDeck
    {
        private readonly ButtonTestDeckRender _betButtonRender;
        private readonly ButtonTestDeckRender _bashButtonRender;
        private readonly LCDButtonDeck _view;
        private readonly LCDButtonDeckViewModel _model;

        public LCDButtonTestDeck()
        {
            _model = new LCDButtonDeckViewModel();
            _view = new LCDButtonDeck();

            var width = _view.Resources["ButtonDeckWidth"] as double? ?? 0;
            var height = _view.Resources["ButtonDeckHeight"] as double? ?? 0;
            _view.DataContext = _model;
            _view.Measure(new System.Windows.Size(width, height));
            _view.Arrange(new System.Windows.Rect(0, 0, width, height));
            _view.UpdateLayout();

            _betButtonRender = new ButtonTestDeckRender(_view.BetButtonGrid, (int)_view.BetButtonGrid.ActualWidth, (int)_view.BetButtonGrid.ActualHeight);
            _bashButtonRender = new ButtonTestDeckRender(_view.BashButton, (int)_view.BashButton.ActualWidth, (int)_view.BashButton.ActualHeight);
        }

        public void OnLoaded()
        {
            Render();
        }

        public void OnUnloaded()
        {
            Render(false);
        }

        public void Pressed(int id)
        {
            SetPressed(id, true);
        }

        public void Released(int id)
        {
            SetPressed(id, false);
        }

        public string ResourceKey(int id)
        {
            return _model.Button(id)?.ResourceKey;
        }

        private void SetPressed(int id, bool b)
        {
            _view.Dispatcher?.Invoke(
                () =>
                {
                    _model.SetButtonEnabled(id, b);
                    _view.UpdateLayout();
                    Render();
                });
        }

        private void Render(bool enabled = true)
        {
            _betButtonRender.Render(0, enabled);
            _bashButtonRender.Render(1, enabled);
        }
    }
}

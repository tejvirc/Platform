namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using Monaco.UI.Common;

    /// <summary>
    ///     Interaction logic for ButtonTestView.xaml
    /// </summary>
    [CLSCompliant(false)]
    public partial class SoundTestPage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SoundTestPage" /> class.
        /// </summary>
        public SoundTestPage()
        {
            InitializeComponent();
            Resources.MergedDictionaries.Add(SkinLoader.Load("CommonUI.xaml"));
        }
    }
}
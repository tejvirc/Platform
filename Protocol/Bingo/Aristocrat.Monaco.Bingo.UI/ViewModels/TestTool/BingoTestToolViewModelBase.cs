namespace Aristocrat.Monaco.Bingo.UI.ViewModels.TestTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using Common.Events;
    using Events;
    using Kernel;
    using Models;
    using MVVM.Command;
    using MVVM.ViewModel;

    public class BingoTestToolViewModelBase : BaseEntityViewModel
    {
        protected readonly IBingoDisplayConfigurationProvider BingoConfigProvider;

        protected bool IsInitializing;

        public BingoTestToolViewModelBase(
            IEventBus eventBus,
            IBingoDisplayConfigurationProvider bingoConfigurationProvider)
        {
            BingoConfigProvider = bingoConfigurationProvider ??
                                   throw new ArgumentNullException(nameof(bingoConfigurationProvider));

            DefaultsCommand = new ActionCommand<object>(_ => SetDefaults());
            LoadCommand = new ActionCommand<object>(_ => Load());
            SaveCommand = new ActionCommand<object>(_ => Save());

            IsInitializing = true;

            eventBus.Subscribe<BingoDisplayConfigurationStartedEvent>(this, _ => SetDefaults());
            eventBus.Subscribe<BingoDisplayConfigurationChangedEvent>(this, _ => SetDefaults());
        }

        public List<BingoWindow> WindowNames => Enum.GetValues(typeof(BingoWindow)).Cast<BingoWindow>().ToList();

        public List<VerticalAlignment> VerticalAlignments => new() { VerticalAlignment.Top, VerticalAlignment.Center, VerticalAlignment.Bottom };

        public List<HorizontalAlignment> HorizontalAlignments => new() { HorizontalAlignment.Left, HorizontalAlignment.Right };

        public List<Orientation> Orientations => new() { Orientation.Horizontal, Orientation.Vertical };

        /// <summary>
        ///     This list of 141 colors is based on Unix X11 standard, as used in .Net Framework, WinForms, Internet Explorer, and WPF.
        /// </summary>
        public List<string> Colors => typeof(Colors).GetProperties().ToList().Select(prop => prop.Name).ToList();

        public ICommand DefaultsCommand { get; set; }

        public ICommand LoadCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        protected virtual void SetDefaults()
        {
            IsInitializing = true;
        }

        protected virtual void Update()
        {
        }

        protected virtual void Load()
        {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            if (System.Windows.Forms.DialogResult.OK != ofd.ShowDialog())
            {
                return;
            }

            BingoConfigProvider.LoadFromFile(ofd.FileName);
        }

        protected virtual void Save()
        {
            var sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "XML-File | *.xml";
            if (System.Windows.Forms.DialogResult.OK != sfd.ShowDialog() ||
                string.IsNullOrWhiteSpace(sfd.FileName))
            {
                return;
            }

            BingoConfigProvider.SaveToFile(sfd.FileName);
        }
    }
}

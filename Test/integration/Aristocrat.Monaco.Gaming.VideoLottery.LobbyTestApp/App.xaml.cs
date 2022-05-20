////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="App.xaml.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2016 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Gaming.VideoLottery.LobbyTestApp
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using Aristocrat.Monaco.Gaming.VideoLottery.UI;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static CompositionContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        public static void Main()
        {
            App app = new App();

            var window = new LobbyView();
            Compose(window);

            window.PreviewKeyDown += Window_PreviewKeyDown;
            app.Run(window);
        }

        private static void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var wnd = sender as Window;
                wnd?.Close();
            }
        }

        private static void Compose(object attributedPart)
        {
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetEntryAssembly()));
            catalog.Catalogs.Add(new DirectoryCatalog(@"."));

            _container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            batch.AddPart(attributedPart);

            try
            {
                _container.Compose(batch);
            }
            catch (CompositionException)
            {
                throw;
            }
        }
    }
}

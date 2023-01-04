namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using Contracts.ConfigWizard;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public class DisplayColorTestsViewModel
    {
        private static readonly IReadOnlyCollection<Color> GrayScaleColors =
            Enumerable.Range(0, 32).Select(x => (byte)(x * 8)).Select(x => Color.FromArgb(255, x, x, x)).ToList();

        private static readonly IReadOnlyCollection<Color> BasicColors = new List<Color>
        {
            Colors.Red, Colors.Green, Colors.Blue, Colors.White
        };

        private static readonly IReadOnlyCollection<Color> BlackPurity = new List<Color> { Colors.Black };

        private static readonly IReadOnlyCollection<Color> WhitePurity = new List<Color> { Colors.White };

        private readonly IInspectionService _reporter;
        private TestData _selectedTest;

        public DisplayColorTestsViewModel(IInspectionService reporter)
        {
            _reporter = reporter;

            SelectedTest = ColorTests.First();
        }

        public IReadOnlyCollection<TestData> ColorTests { get; } = new List<TestData>
        {
            new TestData
            {
                Name = Localize(ResourceKeys.GrayScaleTest),
                Colors = GrayScaleColors,
                Rows = 1,
                Cols = GrayScaleColors.Count
            },
            new TestData
            {
                Name = Localize(ResourceKeys.BasicColorsTest),
                Colors = BasicColors,
                Rows = (int)Math.Sqrt(BasicColors.Count),
                Cols = (int)Math.Sqrt(BasicColors.Count)
            },
            new TestData
            {
                Name = Localize(ResourceKeys.WhitePurityTest),
                Colors = WhitePurity,
                Rows = (int)Math.Sqrt(WhitePurity.Count),
                Cols = (int)Math.Sqrt(WhitePurity.Count)
            },
            new TestData
            {
                Name = Localize(ResourceKeys.BlackPurityTest),
                Colors = BlackPurity,
                Rows = (int)Math.Sqrt(BlackPurity.Count),
                Cols = (int)Math.Sqrt(BlackPurity.Count)
            }
        };

        public TestData SelectedTest
        {
            get => _selectedTest;
            set
            {
                _selectedTest = value;
                _reporter?.SetTestName($"Colors Test: {_selectedTest.Name}");
            }
        }

        private static string Localize(string key)
        {
            return Localizer.For(CultureFor.Operator).GetString(key);
        }

        public class TestData
        {
            public int Rows { get; set; }

            public int Cols { get; set; }

            public string Name { get; set; }

            public IReadOnlyCollection<Color> Colors { get; set; }
        }
    }
}
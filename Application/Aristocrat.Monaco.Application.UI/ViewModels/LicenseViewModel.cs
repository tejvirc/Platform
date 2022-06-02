namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Contracts.Drm;
    using Contracts.Localization;
    using Kernel;
    using Monaco.Localization.Properties;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class LicenseViewModel : OperatorMenuPageViewModelBase
    {
        private readonly IDigitalRights _digitalRights;
        private readonly IPathMapper _pathMapper;
        private const string PackagesPath = "/Packages";
        private const string PackageExtension = @"iso";
        public LicenseViewModel()
        {
            if (InDesigner)
            {
                return;
            }

            _digitalRights = ServiceManager.GetInstance().GetService<IDigitalRights>();
            _pathMapper = ServiceManager.GetInstance().GetService<IPathMapper>();
            TimeRemaining = _digitalRights.TimeRemaining == Timeout.InfiniteTimeSpan
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Unlimited)
                : _digitalRights.TimeRemaining.ToString();

            JurisdictionId = string.IsNullOrEmpty(_digitalRights.JurisdictionId)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable)
                : _digitalRights.JurisdictionId;

            EnabledGamesLimit = _digitalRights.LicenseCount == int.MaxValue
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Unlimited)
                : _digitalRights.LicenseCount.ToString();
        }

        public string Id => _digitalRights.License.Id;

        public Configuration LicenseConfiguration => _digitalRights.License.Configuration;

        public string TimeRemaining { get; }

        public bool HasLicenses => Licenses.Any();

        public IEnumerable<string> Licenses
        {
            get
            {
                var result = _digitalRights.License.Licenses.ToList();
                if (result.Any(x => x == "*"))
                {
                    var packagePath = _pathMapper.GetDirectory(PackagesPath);

                    var isoFiles =
                        Directory.EnumerateFiles(packagePath.FullName, $"*.{PackageExtension}").ToList();
                    
                    if (isoFiles.Any())
                    {
                        result.Clear();
                        foreach (var isoFile in isoFiles)
                        {
                            var fileName = new StringBuilder(Regex.Replace(Path.GetFileNameWithoutExtension(isoFile), @"_[\d][\d.]*", string.Empty));
                            fileName.Append('*');
                            result.Add(fileName.ToString().ToUpper());
                        }
                    }
                }
                return result;
            }
        }

        public string JurisdictionId { get; }

        public string EnabledGamesLimit { get; }
    }
}
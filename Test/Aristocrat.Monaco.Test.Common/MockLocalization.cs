namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Localization.Properties;
    using Moq;

    public static class MockLocalization
    {
        public static Mock<ILocalization> Service { get; private set; }

        public static Mock<ILocalizerFactory> Factory { get; private set; }

        public static Mock<ILocalizer> Localizer { get; private set; }

        public static Mock<ILocalization> Setup(MockBehavior behavior)
        {
            Service = MoqServiceManager.CreateAndAddService<ILocalization>(behavior, true);
            SetFactoryInstance(Service);

            Service.SetupGet(x => x.CurrentCulture)
                .Returns(CultureInfo.GetCultureInfo("en-US"));

            Service.SetupSet(x => x.CurrentCulture = It.IsAny<CultureInfo>());

            var provider = new Mock<ICultureProvider>(behavior);

            provider.SetupGet(x => x.CurrentCulture)
                .Returns(CultureInfo.GetCultureInfo("en-US"));

            provider.Setup(x => x.IsCultureAvailable(It.IsAny<CultureInfo>()))
                .Returns(true);

            provider.Setup(x => x.GetObject<object>(It.IsAny<string>()))
                .Returns(
                    (string key) => GetObject(key, CultureInfo.CurrentCulture));

            provider.Setup(x => x.GetObject<object>(It.IsAny<string>(), It.IsAny<Action<Exception>>()))
                .Returns(
                    (string key) => GetObject(key, CultureInfo.CurrentCulture));

            provider.Setup(x => x.GetObject<string>(It.IsAny<string>()))
                .Returns(
                    (string key) => GetObject(key, CultureInfo.CurrentCulture));

            provider.Setup(x => x.GetObject<string>(It.IsAny<string>(), It.IsAny<Action<Exception>>()))
                .Returns(
                    (string key) => GetObject(key, CultureInfo.CurrentCulture));

            provider.Setup(x => x.GetObject<object>(It.IsAny<CultureInfo>(), It.IsAny<string>()))
                .Returns(
                    (CultureInfo culture, string key) => GetObject(key, culture));

            provider.Setup(x => x.GetObject<object>(It.IsAny<CultureInfo>(), It.IsAny<string>(), It.IsAny<Action<Exception>>()))
                .Returns(
                    (CultureInfo culture, string key) => GetObject(key, culture));

            provider.Setup(x => x.GetObject<string>(It.IsAny<CultureInfo>(), It.IsAny<string>()))
                .Returns(
                    (CultureInfo culture, string key) => GetObject(key, culture));

            provider.Setup(x => x.GetObject<string>(It.IsAny<CultureInfo>(), It.IsAny<string>(), It.IsAny<Action<Exception>>()))
                .Returns(
                    (CultureInfo culture, string key) => GetObject(key, culture));

            provider.Setup(x => x.GetString(It.IsAny<string>()))
                .Returns(
                    (string key) => GetObject(key, CultureInfo.CurrentCulture));

            provider.Setup(x => x.GetString(It.IsAny<string>(), It.IsAny<Action<Exception>>()))
                .Returns(
                    (string key) => GetObject(key, CultureInfo.CurrentCulture));

            provider.Setup(x => x.GetString(It.IsAny<CultureInfo>(), It.IsAny<string>()))
                .Returns(
                    (CultureInfo culture, string key) => GetObject(key, culture));

            provider.Setup(x => x.GetString(It.IsAny<CultureInfo>(), It.IsAny<string>(), It.IsAny<Action<Exception>>()))
                .Returns(
                    (CultureInfo culture, string key) => GetObject(key, culture));

            provider.Setup(x => x.FormatString(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns(
                    (string key, object[] args) =>
                        string.Format(GetObject(key, CultureInfo.CurrentCulture), args));

            provider.Setup(x => x.FormatString(It.IsAny<CultureInfo>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns(
                    (CultureInfo culture, string key, object[] args) =>
                        string.Format(GetObject(key, culture), args));

            Service.Setup(x => x.GetProvider(It.IsAny<string>()))
                .Returns(provider.Object);

            var providerObject = provider.Object;
            Service.Setup(x => x.TryGetProvider(It.IsAny<string>(), out providerObject))
                .Returns(true);

            Localizer = provider.As<ILocalizer>();

            Factory.Setup(x => x.For(It.IsAny<string>()))
                .Returns(Localizer.Object);

            Localizer.Setup(x => x.NewScope())
                .Returns(new CultureScope(Localizer.Object));

            return Service;
        }

        private static string GetObject(string key, CultureInfo culture)
        {
            return Resources.ResourceManager.GetString(key, culture);
        }

        private static string FormatString(string key, object[] args, CultureInfo culture)
        {
            var format = Resources.ResourceManager.GetString(key, culture);
            return string.Format(format, args);
        }

        private static void SetFactoryInstance(Mock<ILocalization> service)
        {
            Factory = service.As<ILocalizerFactory>();
            MoqServiceManager.AddService(Factory);

            var type = typeof(Localizer);
            var field = type.GetField("_factory", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, Factory.Object);
        }
    }
}

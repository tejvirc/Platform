namespace Aristocrat.Monaco.Mgam.Mappings
{
    using System.Linq;
    using System.Reflection;
    using AutoMapper;
    using AutoMapper.Configuration;
    using SimpleInjector;

    /// <summary>
    ///     Registers mapper profiles.
    /// </summary>
    public class MapperProvider
    {
        private readonly Container _container;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MapperProvider"/> class.
        /// </summary>
        /// <param name="container"></param>
        public MapperProvider(Container container)
        {
            _container = container;
        }

        /// <summary>
        ///     Creates an <see cref="IMapper"/> instance.
        /// </summary>
        /// <returns><see cref="IMapper"/>.</returns>
        public IMapper GetMapper()
        {
            var mce = new MapperConfigurationExpression();
            mce.ConstructServicesUsing(t => _container.GetInstance(t));

            var types =
                from type in Assembly.GetExecutingAssembly().GetExportedTypes()
                where typeof(Profile).IsAssignableFrom(type) && !type.IsAbstract
                select type;

            foreach (var type in types)
            {
                mce.AddProfile(type);
            }

            var mc = new MapperConfiguration(mce);

            mc.AssertConfigurationIsValid();

            return new Mapper(mc, t => _container.GetInstance(t));
        }
    }
}

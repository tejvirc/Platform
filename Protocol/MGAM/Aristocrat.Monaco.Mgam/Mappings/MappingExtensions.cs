namespace Aristocrat.Monaco.Mgam.Mappings
{
    using System;
    using AutoMapper;

    /// <summary>
    ///     A set of <see cref="IMapper" /> extension methods
    /// </summary>
    public static class MappingExtensions
    {
        public static TResult MergeInto<TResult>(this IMapper mapper, object source1, object source2)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            if (source1 == null)
            {
                throw new ArgumentNullException(nameof(source1));
            }

            if (source2 == null)
            {
                throw new ArgumentNullException(nameof(source2));
            }

            return mapper.Map(source2, mapper.Map<TResult>(source1));
        }
    }
}
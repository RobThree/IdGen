using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace IdGen.DependencyInjection
{
    public static class IdGenServiceCollectionExtensions
    {
        public static IServiceCollection AddIdGen(this IServiceCollection services, int generatorId)
            => AddIdGen(services, generatorId, () => IdGeneratorOptions.Default);

        public static IServiceCollection AddIdGen(this IServiceCollection services, int generatorId, Func<IdGeneratorOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.TryAddSingleton<IIdGenerator<long>>(new IdGenerator(generatorId, options()));
            services.TryAddSingleton<IdGenerator>(c => (IdGenerator)c.GetRequiredService<IIdGenerator<long>>());

            return services;
        }
    }
}

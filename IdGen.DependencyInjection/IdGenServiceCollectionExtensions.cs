using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace IdGen.DependencyInjection;

/// <summary>
/// Helper class to integrate IdGen with Microsoft.Extensions.DependencyInjection
/// </summary>
public static class IdGenServiceCollectionExtensions
{
    /// <summary>
    /// Registers a singleton <see cref="IdGenerator"/> with the given <paramref name="generatorId"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register the singleton <see cref="IdGenerator"/> on.</param>
    /// <param name="generatorId">The generator-id to use for the singleton <see cref="IdGenerator"/>.</param>
    /// <returns>The given <see cref="IServiceCollection"/> with the registered singleton in it.</returns>
    public static IServiceCollection AddIdGen(this IServiceCollection services, int generatorId)
        => AddIdGen(services, generatorId, () => IdGeneratorOptions.Default);

    /// <summary>
    /// Registers a singleton <see cref="IdGenerator"/> with the given <paramref name="generatorId"/> and <see cref="IdGeneratorOptions"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register the singleton <see cref="IdGenerator"/> on.</param>
    /// <param name="generatorId">The generator-id to use for the singleton <see cref="IdGenerator"/>.</param>
    /// <param name="options">The <see cref="IdGeneratorOptions"/> for the singleton <see cref="IdGenerator"/>.</param>
    /// <returns>The given <see cref="IServiceCollection"/> with the registered singleton <see cref="IdGenerator"/> in it.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null</exception>
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

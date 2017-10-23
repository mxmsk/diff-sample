using System;
using DiffSample.Abstractions;
using DiffSample.Abstractions.Queue;
using DiffSample.Abstractions.Storage;
using DiffSample.Services.Algorithms;
using DiffSample.Services.Queue;
using DiffSample.Services.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace DiffSample.Services.Configuration
{
    /// <summary>
    /// Exposes helper methods for registering difference services.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Adds difference services that use internal queue and binary comparison.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> with added services.</returns>
        public static IServiceCollection AddBinaryDifferencePipeline(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services
                .AddSingleton<IDifferenceAlgorithm, BinaryAlgorithm>()
                .AddSingleton<IDifferenceService, DifferenceService>()
                .AddSingleton<IDifferenceQueue, DifferenceQueue>()
                .AddSingleton<IStorage, DiskStorage>()
                .AddSingleton<IQueueConsumer, SourceConsumer>()
                .AddSingleton<IQueueConsumer, ReadyConsumer>();
        }
    }
}

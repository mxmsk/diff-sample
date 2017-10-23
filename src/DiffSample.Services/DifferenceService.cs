using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using DiffSample.Abstractions.Queue;
using DiffSample.Abstractions.Storage;

namespace DiffSample.Services
{
    /// <summary>
    /// Implements queue-based difference service.
    /// </summary>
    public class DifferenceService : IDifferenceService
    {
        private readonly IDifferenceQueue _queue;
        private readonly IEnumerable<IQueueConsumer> _queueConsumers;
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferenceService"/> class.
        /// </summary>
        /// <param name="queue">The difference queue implementation.</param>
        /// <param name="queueConsumers">Registered difference queue consumers.</param>
        /// <param name="storage">The object representing persistent storage.</param>
        public DifferenceService(IDifferenceQueue queue,
                                 IEnumerable<IQueueConsumer> queueConsumers,
                                 IStorage storage)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _queueConsumers = queueConsumers ?? throw new ArgumentNullException(nameof(queueConsumers));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>
        /// Adds the content of the source.
        /// </summary>
        /// <param name="diffId">Identifier of diff on which the source can be
        /// retrieved in a future.</param>
        /// <param name="content">Source's content to add.</param>
        /// <returns>The task that adds the content.</returns>
        public Task AddSourceAsync(int diffId, SourceContent content) => 
            _queue.PushSourceAsync(content.InDiffBag(diffId));

        /// <summary>
        /// Finds diff by its identifier.
        /// </summary>
        /// <param name="diffId">Identifier of diff to find.</param>
        /// <returns>The task that finds the diff; the readiness shows availability of diff.</returns>
        public Task<(DifferenceContent, DifferenceReadiness)> FindDiffAsync(int diffId) => 
            _storage.LoadDiffAsync(diffId);

        /// <summary>
        /// Starts internals of difference pipeline.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for pipeline.</param>
        /// <returns>The task representing running pipeline.</returns>
        public Task RunAsync(CancellationToken cancellationToken) => Task.WhenAll(
            _queueConsumers.Select(c => c.RunAsync(TaskPoolScheduler.Default, cancellationToken)));
    }
}

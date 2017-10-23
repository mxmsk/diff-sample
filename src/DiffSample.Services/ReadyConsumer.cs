using System;
using DiffSample.Abstractions;
using DiffSample.Abstractions.Queue;
using DiffSample.Abstractions.Storage;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace DiffSample.Services
{
    /// <summary>
    /// The consumer that subscribes to difference queue to handle sources
    /// ready for calculating the difference.
    /// </summary>
    public class ReadyConsumer : IQueueConsumer
    {
        private readonly IDifferenceQueue _queue;
        private readonly IStorage _storage;
        private readonly IDifferenceAlgorithm _algorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadyConsumer"/> class.
        /// </summary>
        /// <param name="queue">The difference queue implementation.</param>
        /// <param name="storage">The object representing persistent storage.</param>
        /// <param name="algorithm">The algorithm to apply to sources in order to get diff.</param>
        public ReadyConsumer(IDifferenceQueue queue, IStorage storage, IDifferenceAlgorithm algorithm)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
        }

        /// <summary>
        /// Runs consumer on the specified scheduler.
        /// </summary>
        /// <param name="scheduler">The scheduler that consumer must run on.</param>
        /// <param name="cancellationToken">The token for stopping consumer.</param>
        /// <returns>The task that runs the comsumer.</returns>
        public Task RunAsync(IScheduler scheduler, CancellationToken cancellationToken)
        {
            _queue.ReadyContents
                  .ObserveOn(scheduler)
                  .SelectMany(async content =>
                  {
                      await OnReadyContentAsync(content);
                      return Unit.Default;
                  })
                  .Subscribe(cancellationToken);

            return Task.CompletedTask;
        }

        private async Task OnReadyContentAsync(DiffBag<IEnumerable<SourceContent>> content)
        {
            var diff = await _algorithm.GetDiffAsync(content.Data);
            await _storage.SaveDiffAsync(diff.InDiffBag(content.DiffId));
        }
    }
}
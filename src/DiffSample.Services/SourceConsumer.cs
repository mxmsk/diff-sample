using System;
using DiffSample.Abstractions;
using DiffSample.Abstractions.Queue;
using DiffSample.Abstractions.Storage;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace DiffSample.Services
{
    /// <summary>
    /// The consumer that subscribes to difference queue to handle new sources.
    /// </summary>
    public class SourceConsumer : IQueueConsumer
    {
        private readonly IDifferenceQueue _queue;
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceConsumer"/> class.
        /// </summary>
        /// <param name="queue">The difference queue implementation.</param>
        /// <param name="storage">The object representing persistent storage.</param>
        public SourceConsumer(IDifferenceQueue queue, IStorage storage)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>
        /// Runs consumer on the specified scheduler.
        /// </summary>
        /// <param name="scheduler">The scheduler that consumer must run on.</param>
        /// <param name="cancellationToken">The token for stopping consumer.</param>
        /// <returns>The task that runs the comsumer.</returns>
        public Task RunAsync(IScheduler scheduler, CancellationToken cancellationToken)
        {
            _queue.SourceContents
                  .ObserveOn(scheduler)
                  .SelectMany(async content =>
                  {
                      await OnSourceContentAsync(content);
                      return Unit.Default;
                  })
                  .Subscribe(cancellationToken);

            return Task.CompletedTask;
        }

        private async Task OnSourceContentAsync(DiffBag<SourceContent> content)
        {
            var otherSide = content.Data.SourceSide == SourceSide.Left
                                   ? SourceSide.Right : SourceSide.Left;

            var (otherSideContent, otherSideExists) = await _storage.LoadSourceAsync(content.DiffId, otherSide);
            if (otherSideExists)
            {
                // Do not save received content if both sides are available.
                // It is a subject to change. We must save content when we have
                // an ability to retry to build diff after system failure.
                var readyContent = new[] { content.Data, otherSideContent };
                await _queue.PushReadyAsync(readyContent.AsEnumerable().InDiffBag(content.DiffId));

                return;
            }

            await _storage.SaveSourceAsync(content);
        }
    }
}
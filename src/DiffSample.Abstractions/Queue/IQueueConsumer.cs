using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace DiffSample.Abstractions.Queue
{
    /// <summary>
    /// Abstracts queue consumer that will be started by <see cref="IDifferenceService"/>.
    /// </summary>
    public interface IQueueConsumer
    {
        /// <summary>
        /// Runs consumer on the specified scheduler.
        /// </summary>
        /// <param name="scheduler">The scheduler that consumer must run on.</param>
        /// <param name="cancellationToken">The token for stopping consumer.</param>
        /// <returns>The task that runs the comsumer.</returns>
        Task RunAsync(IScheduler scheduler, CancellationToken cancellationToken);
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace DiffSample.Abstractions
{
    /// <summary>
    /// Represents service that accepts data and provides their difference.
    /// </summary>
    public interface IDifferenceService
    {
        /// <summary>
        /// Adds the content of the source.
        /// </summary>
        /// <param name="diffId">Identifier of diff on which the source can be
        /// retrieved in a future.</param>
        /// <param name="content">Source's content to add.</param>
        /// <returns>The task that adds the content.</returns>
        Task AddSourceAsync(int diffId, SourceContent content);

        /// <summary>
        /// Finds diff by its identifier.
        /// </summary>
        /// <param name="diffId">Identifier of diff to find.</param>
        /// <returns>The task that finds the diff; the readiness shows availability of diff.</returns>
        Task<(DifferenceContent, DifferenceReadiness)> FindDiffAsync(int diffId);

        /// <summary>
        /// Starts internals of difference pipeline.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for pipeline.</param>
        /// <returns>The task representing running pipeline.</returns>
        Task RunAsync(CancellationToken cancellationToken);
    }
}

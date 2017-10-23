using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiffSample.Abstractions
{
    /// <summary>
    /// Abstracts an algorithm that produces diffs of given sources.
    /// </summary>
    public interface IDifferenceAlgorithm
    {
        /// <summary>
        /// Compares sources and generates the difference.
        /// </summary>
        /// <param name="contents">Contents to be compared.</param>
        /// <remarks>
        /// Algorithms are introduced for unlimited count of sources.
        /// It is algorithm's responsibility to check whether sources are valid for it.
        /// </remarks>
        /// <returns>The task that generates the difference.</returns>
        Task<DifferenceContent> GetDiffAsync(IEnumerable<SourceContent> contents);
    }
}
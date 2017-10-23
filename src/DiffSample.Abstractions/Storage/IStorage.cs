using System.Threading.Tasks;

namespace DiffSample.Abstractions.Storage
{
    /// <summary>
    /// Defines the interface of persistent storage.
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// Saves the content of the source in the storage.
        /// </summary>
        /// <param name="content">The content to save.</param>
        /// <returns>The task that saves the content.</returns>
        Task SaveSourceAsync(DiffBag<SourceContent> content);

        /// <summary>
        /// Loads content of source by diff identifier and side.
        /// </summary>
        /// <param name="diffId">Identifier of diff which the source belongs to.</param>
        /// <param name="side">The side a content had come from.</param>
        /// <returns>The task that loads content of the source.</returns>
        Task<(SourceContent, bool)> LoadSourceAsync(int diffId, SourceSide side);

        /// <summary>
        /// Saves the content of the diff in the storage.
        /// </summary>
        /// <param name="content">The content to save.</param>
        /// <returns>The task that saves the content.</returns>
        Task SaveDiffAsync(DiffBag<DifferenceContent> content);

        /// <summary>
        /// Loads diff by its identifier.
        /// </summary>
        /// <param name="diffId">Identifier of diff to load.</param>
        /// <returns>The task that finds the diff; the readiness shows availability of diff.</returns>
        Task<(DifferenceContent, DifferenceReadiness)> LoadDiffAsync(int diffId);
    }
}

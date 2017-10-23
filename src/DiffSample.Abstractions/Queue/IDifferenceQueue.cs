using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiffSample.Abstractions.Queue
{
    /// <summary>
    /// Provides queueing interface for difference pipeline.
    /// </summary>
    public interface IDifferenceQueue
    {
        /// <summary>
        /// Pushes the content of the source into queue.
        /// </summary>
        /// <param name="content">Source's content to push.</param>
        /// <returns>The task that pushes the content into queue.</returns>
        Task PushSourceAsync(DiffBag<SourceContent> content);

        /// <summary>
        /// Pushes contents ready for diff into queue.
        /// </summary>
        /// <param name="contents">Contents to push into queue.</param>
        /// <returns>The task that pushes contents into queue.</returns>
        Task PushReadyAsync(DiffBag<IEnumerable<SourceContent>> contents);

        /// <summary>
        /// Gets the sequence of contents received from sources on queue.
        /// </summary>
        IObservable<DiffBag<SourceContent>> SourceContents { get; }

        /// <summary>
        /// Gets the sequence of contents received on queue for difference.
        /// </summary>
        IObservable<DiffBag<IEnumerable<SourceContent>>> ReadyContents { get; }
    }
}

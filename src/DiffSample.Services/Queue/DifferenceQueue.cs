using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using DiffSample.Abstractions.Queue;

namespace DiffSample.Services.Queue
{
    using SourceContentMessage = DiffBag<SourceContent>;
    using ReadyContentMessage = DiffBag<IEnumerable<SourceContent>>;

    /// <summary>
    /// Implements difference queue that just pushes a given items
    /// to corresponding observables.
    /// </summary>
    /// <remarks>
    /// The queue is pretty simple here. It can be replaced with full-functional
    /// MQ service that is durable and retriable solution. In this case DifferenceQueue
    /// can act like consumer and producer simultaneously.
    /// </remarks>
    public class DifferenceQueue : IDifferenceQueue
    {
        private readonly ISubject<SourceContentMessage> _sourceContents = new Subject<SourceContentMessage>();
        private readonly ISubject<ReadyContentMessage> _readyContents = new Subject<ReadyContentMessage>();

        /// <summary>
        /// Pushes the content of the source into queue.
        /// </summary>
        /// <param name="content">Source's content to push.</param>
        /// <returns>The task that pushes the content into queue.</returns>
        public Task PushSourceAsync(SourceContentMessage content)
        {
            _sourceContents.OnNext(content);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Pushes contents ready for diff into queue.
        /// </summary>
        /// <param name="contents">Contents to push into queue.</param>
        /// <returns>The task that pushes contents into queue.</returns>
        public Task PushReadyAsync(ReadyContentMessage contents)
        {
            _readyContents.OnNext(contents);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the sequence of contents received from sources on queue.
        /// </summary>
        public IObservable<SourceContentMessage> SourceContents => _sourceContents;

        /// <summary>
        /// Gets the sequence of contents received on queue for difference.
        /// </summary>
        public IObservable<ReadyContentMessage> ReadyContents => _readyContents;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using DiffSample.Services.Queue;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffSample.Services.Test.Queue
{
    [TestClass]
    public class DifferenceQueueTest
    {
        [TestMethod]
        public async Task PushesSourceContentToSourceContentsObservable()
        {
            var content = new SourceContent
            {
                SourceSide = SourceSide.Right,
                Data = new byte[] { 1, 2, 3 }
            };

            DiffBag<SourceContent> received = null;

            var queue = new DifferenceQueue();
            queue.SourceContents.Subscribe(c => received = c);
            await queue.PushSourceAsync(content.InDiffBag(10));

            received.ShouldBeEquivalentTo(content.InDiffBag(10));
        }

        [TestMethod]
        public async Task PushesReadyToReadyContentsObservable()
        {
            var content = new[]
            {
                new SourceContent
                {
                    SourceSide = SourceSide.Left,
                    Data = new byte[] { 1, 2, 3 }
                },
                new SourceContent
                {
                    SourceSide = SourceSide.Right,
                    Data = new byte[] { 3, 4, 5 }
                }
            }.AsEnumerable();

            DiffBag<IEnumerable<SourceContent>> received = null;

            var queue = new DifferenceQueue();
            queue.ReadyContents.Subscribe(r => received = r);
            await queue.PushReadyAsync(content.InDiffBag(10));

            received.ShouldBeEquivalentTo(content.InDiffBag(10));
        }
    }
}

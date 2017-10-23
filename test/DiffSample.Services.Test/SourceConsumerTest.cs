using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using DiffSample.Abstractions.Queue;
using DiffSample.Abstractions.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Collections.Generic;
using System.Threading;

namespace DiffSample.Services.Test
{
    [TestClass]
    public class SourceConsumerTest
    {
        [TestMethod]
        public async Task SavesFirstSourceInStorage()
        {
            var content = new SourceContent
            {
                SourceSide = SourceSide.Left,
                Data = new byte[] { 1, 2, 3 }
            };

            var queue = Substitute.For<IDifferenceQueue>();
            var storage = Substitute.For<IStorage>();

            queue.SourceContents.Returns(Observable.Return(content.InDiffBag(5)));

            var consumer = new SourceConsumer(queue, storage);
            await consumer.RunAsync(Scheduler.Immediate, CancellationToken.None);

            await storage.Received().SaveSourceAsync(content.InDiffBag(5));
        }

        [TestMethod]
        public async Task PushesContentToQueueWhenBothSidesReceived()
        {
            var content1 = new SourceContent
            {
                SourceSide = SourceSide.Left,
                Data = new byte[] { 1, 2, 3 }
            };
            var content2 = new SourceContent
            {
                SourceSide = SourceSide.Right,
                Data = new byte[] { 4, 5, 6 }
            };
            var ready = new[] { content1, content2 }.AsEnumerable();

            var queue = Substitute.For<IDifferenceQueue>();
            var storage = Substitute.For<IStorage>();

            storage.LoadSourceAsync(5, content2.SourceSide)
                   .Returns(Task.FromResult((content2, true)));
            queue.SourceContents
                 .Returns(Observable.Return(content1.InDiffBag(5)));

            var consumer = new SourceConsumer(queue, storage);
            await consumer.RunAsync(Scheduler.Immediate, CancellationToken.None);

            await queue.Received().PushReadyAsync(Arg.Is<DiffBag<IEnumerable<SourceContent>>>(
                pushed => pushed.DiffId == 5 && pushed.Data.SequenceEqual(ready)));
        }

        [TestMethod]
        public async Task DoesntPushContentToQueueWhenOnlyOneSideReceived()
        {
            var content1 = new SourceContent
            {
                SourceSide = SourceSide.Left,
                Data = new byte[] { 1, 2, 3 }
            };
            var content2 = new SourceContent
            {
                SourceSide = SourceSide.Left,
                Data = new byte[] { 4, 5, 6 }
            };

            var queue = Substitute.For<IDifferenceQueue>();
            var storage = Substitute.For<IStorage>();

            queue.SourceContents.Returns(
                Observable.Return(content1.InDiffBag(5))
                .Concat(Observable.Return(content2.InDiffBag(5))));

            var consumer = new SourceConsumer(queue, storage);
            await consumer.RunAsync(Scheduler.Immediate, CancellationToken.None);

            await queue.DidNotReceiveWithAnyArgs().PushReadyAsync(null);
        }

        [TestMethod]
        public async Task DoesntSaveOtherSideInStorageWhenBothSidesReceived()
        {
            var content1 = new SourceContent
            {
                SourceSide = SourceSide.Left,
                Data = new byte[] { 1, 2, 3 }
            };
            var content2 = new SourceContent
            {
                SourceSide = SourceSide.Right,
                Data = new byte[] { 4, 5, 6 }
            };
            var ready = new[] { content1, content2 }.AsEnumerable();

            var queue = Substitute.For<IDifferenceQueue>();
            var storage = Substitute.For<IStorage>();

            storage.LoadSourceAsync(5, content2.SourceSide)
                   .Returns(Task.FromResult((content2, true)));
            queue.SourceContents
                 .Returns(Observable.Return(content1.InDiffBag(5)));

            var consumer = new SourceConsumer(queue, storage);
            await consumer.RunAsync(Scheduler.Immediate, CancellationToken.None);

            await storage.DidNotReceiveWithAnyArgs().SaveSourceAsync(null);
        }
    }
}

using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using DiffSample.Abstractions.Queue;
using DiffSample.Abstractions.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Threading;

namespace DiffSample.Services.Test
{
    [TestClass]
    public class ReadyConsumerTest
    {
        [TestMethod]
        public async Task AppliesAlgorithmToSources()
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

            var queue = Substitute.For<IDifferenceQueue>();
            var storage = Substitute.For<IStorage>();
            var algorithm = Substitute.For<IDifferenceAlgorithm>();

            queue.ReadyContents.Returns(Observable.Return(content.InDiffBag(5)));

            var consumer = new ReadyConsumer(queue, storage, algorithm);
            await consumer.RunAsync(Scheduler.Immediate, CancellationToken.None);

            await algorithm.Received().GetDiffAsync(content);
        }

        [TestMethod]
        public async Task SavesDifferenceContentInStorage()
        {
            var ready = new[]
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

            var diff = new DifferenceContent
            {
                Type = DifferenceType.Detailed,
                Details = new[] {
                    new DifferenceDetail{ LeftOffset = 1, RightOffset = 2 },
                    new DifferenceDetail{ LeftLength = 3, RightLength = 4 },
                }
            };

            var queue = Substitute.For<IDifferenceQueue>();
            var storage = Substitute.For<IStorage>();
            var algorithm = Substitute.For<IDifferenceAlgorithm>();

            queue.ReadyContents.Returns(Observable.Return(ready.InDiffBag(5)));
            algorithm.GetDiffAsync(ready).Returns(Task.FromResult(diff));

            var consumer = new ReadyConsumer(queue, storage, algorithm);
            await consumer.RunAsync(Scheduler.Immediate, CancellationToken.None);

            await storage.Received().SaveDiffAsync(diff.InDiffBag(5));
        }
    }
}

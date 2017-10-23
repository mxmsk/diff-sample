using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using DiffSample.Abstractions.Queue;
using DiffSample.Abstractions.Storage;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace DiffSample.Services.Test
{
    [TestClass]
    public class DifferenceServiceTest
    {
        [TestMethod]
        public async Task PushesSourceContentToQueue()
        {
            var content = new SourceContent
            {
                Data = new byte[]{ 1, 2, 3 },
                SourceSide = SourceSide.Left
            };
            
            var queue = Substitute.For<IDifferenceQueue>();
            var storage = Substitute.For<IStorage>();

            var service = new DifferenceService(queue, Enumerable.Empty<IQueueConsumer>(), storage);
            await service.AddSourceAsync(10, content);

            await queue.Received().PushSourceAsync(content.InDiffBag(10));
        }

        [DataTestMethod]
        [DataRow(DifferenceType.Equal, DifferenceReadiness.Ready, 1)]
        [DataRow(DifferenceType.Detailed, DifferenceReadiness.Ready, 2)]
        [DataRow(DifferenceType.Equal, DifferenceReadiness.NotReady, 3)]
        [DataRow(DifferenceType.Equal, DifferenceReadiness.NotFound, 3)]
        public async Task FindsDiffInStorage(DifferenceType diffType,
                                             DifferenceReadiness diffReadiness, int leftOffset)
        {
            var queue = Substitute.For<IDifferenceQueue>();
            var storage = Substitute.For<IStorage>();

            var diff = new DifferenceContent
            {
                Type = diffType,
                Details = new[] { new DifferenceDetail { LeftOffset = leftOffset } },
            };
            storage.LoadDiffAsync(10).Returns(Task.FromResult((diff, diffReadiness)));

            var service = new DifferenceService(queue, Enumerable.Empty<IQueueConsumer>(), storage);
            var (found, readiness) = await service.FindDiffAsync(10);

            found.Type.Should().Be(diffType);
            found.Details.Should().ContainSingle(d => d.LeftOffset == leftOffset);
            readiness.Should().Be(diffReadiness);
        }

        [TestMethod]
        public async Task RunsQueueConsumersOnTaskPoolScheduler()
        {
            var queue = Substitute.For<IDifferenceQueue>();
            var storage = Substitute.For<IStorage>();

            var consumer1 = Substitute.For<IQueueConsumer>();
            var consumer2 = Substitute.For<IQueueConsumer>();

            var cancellationToken = new CancellationToken();

            var service = new DifferenceService(queue, new[] { consumer1, consumer2 }, storage);
            await service.RunAsync(cancellationToken);

            await consumer1.Received().RunAsync(TaskPoolScheduler.Default, cancellationToken);
            await consumer2.Received().RunAsync(TaskPoolScheduler.Default, cancellationToken);
        }
    }
}

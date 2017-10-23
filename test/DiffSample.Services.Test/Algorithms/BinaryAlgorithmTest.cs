using System.Linq;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiffSample.Services.Algorithms;
using System.Collections.Generic;

namespace DiffSample.Services.Test.Algorithm
{
    [TestClass]
    public class BinaryAlgorithmTest
    {
        public static IEnumerable<SourceContent> MakeContents(
            IEnumerable<int> left, IEnumerable<int> right)
        {
            return new[] {
                new SourceContent
                {
                    SourceSide = SourceSide.Left,
                    Data = left.Select(i => (byte)i).ToArray(),
                },
                new SourceContent
                {
                    SourceSide = SourceSide.Right,
                    Data = right.Select(i => (byte)i).ToArray(),
                },
            };
        }
        
        [TestMethod]
        public async Task ReturnsTypeEqualForEmptyContent()
        {
            var contents = MakeContents(
                Enumerable.Empty<int>(), Enumerable.Empty<int>());

            var alg = new BinaryAlgorithm();
            var diff = await alg.GetDiffAsync(contents);

            diff.Type.Should().Be(DifferenceType.Equal);
            diff.Details.Should().BeEmpty();
        }

        [DataTestMethod]
        [DataRow(3, 0)]
        [DataRow(0, 2)]
        [DataRow(3, 2)]
        public async Task ReturnsTypeSizeDiffersWhenLengthDifferent(int leftLength, int rightLength)
        {
            var contents = MakeContents(
                Enumerable.Repeat(10, leftLength),
                Enumerable.Repeat(10, rightLength));

            var alg = new BinaryAlgorithm();
            var diff = await alg.GetDiffAsync(contents);

            diff.Type.Should().Be(DifferenceType.SizeDiffers);
            diff.Details.Should().BeEmpty();
        }

        [TestMethod]
        public async Task ReturnsTypeEqualForEqualContent()
        {
            var contents = MakeContents(
                new[] { 0, 1, 2, 3, 4, 5, 6, 7 },
                new[] { 0, 1, 2, 3, 4, 5, 6, 7 });

            var alg = new BinaryAlgorithm();
            var diff = await alg.GetDiffAsync(contents);

            diff.Type.Should().Be(DifferenceType.Equal);
            diff.Details.Should().BeEmpty();
        }

        [TestMethod]
        public async Task ReturnsTypeDetailed1()
        {
            var contents = MakeContents(
                new[] { 0, 1, 2, 3, 4, 5, 6, 9 },
                new[] { 0, 1, 4, 5, 6, 7, 8, 9 });

            var alg = new BinaryAlgorithm();
            var diff = await alg.GetDiffAsync(contents);

            diff.Type.Should().Be(DifferenceType.Detailed);
            diff.Details.Should()
                .HaveCount(2)
                .And.Contain(d => 
                             d.LeftOffset == 2 && d.LeftLength == 2 &&
                             d.RightOffset == 2 && d.RightLength == 0)
                .And.Contain(d =>
                             d.LeftOffset == 7 && d.LeftLength == 1 &&
                             d.RightOffset == 5 && d.RightLength == 3);
        }

        [TestMethod]
        public async Task ReturnsTypeDetailed2()
        {
            var contents = MakeContents(
                new[] { 1, 2, 3, 4, 5, 6, 7, 8 },
                new[] { 0, 0, 1, 2, 3, 6, 7, 8 });

            var alg = new BinaryAlgorithm();
            var diff = await alg.GetDiffAsync(contents);

            diff.Type.Should().Be(DifferenceType.Detailed);
            diff.Details.Should()
                .HaveCount(2)
                .And.Contain(d =>
                             d.LeftOffset == 0 && d.LeftLength == 0 &&
                             d.RightOffset == 0 && d.RightLength == 2)
                .And.Contain(d =>
                             d.LeftOffset == 3 && d.LeftLength == 2 &&
                             d.RightOffset == 5 && d.RightLength == 0);
        }

        [TestMethod]
        public async Task ReturnsTypeDetailed3()
        {
            var contents = MakeContents(
                new[] { 0, 5, 6, 7, 8, },
                new[] { 0, 0, 0, 1, 2, });

            var alg = new BinaryAlgorithm();
            var diff = await alg.GetDiffAsync(contents);

            diff.Type.Should().Be(DifferenceType.Detailed);
            diff.Details.Should()
                .HaveCount(1)
                .And.Contain(d =>
                             d.LeftOffset == 1 && d.LeftLength == 4 &&
                             d.RightOffset == 1 && d.RightLength == 4);
        }

        [TestMethod]
        public async Task ReturnsTypeDetailed4()
        {
            var contents = MakeContents(
                new[] { 1, 2, 3, 4, 5 },
                new[] { 1, 2, 3, 4, 6 });

            var alg = new BinaryAlgorithm();
            var diff = await alg.GetDiffAsync(contents);

            diff.Type.Should().Be(DifferenceType.Detailed);
            diff.Details.Should()
                .HaveCount(1)
                .And.Contain(d =>
                             d.LeftOffset == 4 && d.LeftLength == 1 &&
                             d.RightOffset == 4 && d.RightLength == 1);
        }
    }
}

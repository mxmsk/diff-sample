using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using DiffSample.Abstractions.Configuration;
using DiffSample.Services.Storage;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DiffSample.Services.Test.Storage
{
    [TestClass]
    public class DiskStorageTest
    {
        private static string _DataDir = Path.GetTempPath();
        private static readonly IOptions<DifferenceOptions> _Options =
            Options.Create(new DifferenceOptions { DataDir = _DataDir });
        
        [DataTestMethod]
        [DataRow(1, SourceSide.Left)]
        [DataRow(2, SourceSide.Right)]
        [DataRow(2, SourceSide.Left)]
        [DataRow(1, SourceSide.Right)]
        public async Task SavesSourceToDataDir(int diffId, SourceSide side)
        {
            var content = new SourceContent
            {
                Data = new byte[] { 1, 2, 3 },
                SourceSide = side
            };
            var expectedFileName = Path.Combine(_DataDir, string.Concat(
                diffId.ToString(), ".", side.ToString().ToLowerInvariant()));

            File.Delete(expectedFileName);

            var storage = new DiskStorage(_Options);
            await storage.SaveSourceAsync(content.InDiffBag(diffId));

            File.ReadAllBytes(expectedFileName)
                .Should().Equal(new byte[] { 1, 2, 3 });
        }

        [DataTestMethod]
        [DataRow(3, SourceSide.Left)]
        [DataRow(4, SourceSide.Right)]
        [DataRow(4, SourceSide.Left)]
        [DataRow(3, SourceSide.Right)]
        public async Task LoadsSourceFromDataDir(int diffId, SourceSide side)
        {
            var data = new byte[] { 3, 4, 5 };
            var expectedFileName = Path.Combine(_DataDir, string.Concat(
                diffId.ToString(), ".", side.ToString().ToLowerInvariant()));

            File.WriteAllBytes(expectedFileName, data);
            try
            {
                var storage = new DiskStorage(_Options);
                var (source, exists) = await storage.LoadSourceAsync(diffId, side);

                source.SourceSide.Should().Be(side);
                source.Data.Should().Equal(data);
                exists.Should().BeTrue();
            }
            finally
            {
                File.Delete(expectedFileName);
            }
        }

        [TestMethod]
        public async Task ReturnsFalseForSourceThatDoesntExistsInDataDir()
        {
            var data = new byte[] { 3, 4, 5 };
            var expectedFileName = Path.Combine(_DataDir, "5.left");

            File.Delete(expectedFileName);

            var storage = new DiskStorage(_Options);
            var (_, exists) = await storage.LoadSourceAsync(5, SourceSide.Left);

            exists.Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow(1, DifferenceType.Detailed, 3)]
        [DataRow(2, DifferenceType.SizeDiffers, 4)]
        public async Task SavesDiffToDataDir(int diffId, DifferenceType type, int offset)
        {
            var content = new DifferenceContent
            {
                Type = type,
                Details = new[] {
                    new DifferenceDetail { LeftOffset = offset },
                    new DifferenceDetail { RightOffset = offset },
                }
            };
            var expectedFileName = Path.Combine(_DataDir,
                                                string.Concat(diffId.ToString(), ".diff"));

            File.Delete(expectedFileName);

            var storage = new DiskStorage(_Options);
            await storage.SaveDiffAsync(content.InDiffBag(diffId));

            File.ReadAllText(expectedFileName)
                .Should().Be(JsonConvert.SerializeObject(content));
        }

        [DataTestMethod]
        [DataRow(3, DifferenceType.Equal, 5)]
        [DataRow(4, DifferenceType.SizeDiffers, 6)]
        public async Task LoadsReadyDiffFromDataDir(int diffId, DifferenceType type, int length)
        {
            var content = new DifferenceContent
            {
                Type = type,
                Details = new[] {
                    new DifferenceDetail { LeftLength = length },
                    new DifferenceDetail { RightLength = length },
                }
            };
            var expectedFileName = Path.Combine(_DataDir,
                                                string.Concat(diffId.ToString(), ".diff"));

            File.WriteAllText(expectedFileName, JsonConvert.SerializeObject(content));
            try
            {
                var storage = new DiskStorage(_Options);
                var (diff, readiness) = await storage.LoadDiffAsync(diffId);

                readiness.Should().Be(DifferenceReadiness.Ready);

                diff.Type.Should().Be(type);
                diff.Details.ShouldAllBeEquivalentTo(content.Details,
                                                     opts => opts.WithStrictOrdering());
            }
            finally
            {
                File.Delete(expectedFileName);
            }
        }

        [TestMethod]
        public async Task ReturnsNotFoundForDiffThatDoesntExistsInDataDirAlongWithSource()
        {
            var data = new byte[] { 3, 4, 5 };
            var expectedFileNames = new List<string>
            {
                Path.Combine(_DataDir, "10.left"),
                Path.Combine(_DataDir, "10.right"),
                Path.Combine(_DataDir, "10.diff"),
            };

            expectedFileNames.ForEach(File.Delete);

            var storage = new DiskStorage(_Options);
            var (_, readiness) = await storage.LoadDiffAsync(10);

            readiness.Should().Be(DifferenceReadiness.NotFound);
        }

        [DataTestMethod]
        [DataRow(SourceSide.Left)]
        [DataRow(SourceSide.Right)]
        public async Task ReturnsNotReadyForDiffThatDoesntExistsInDataDirButHasSource(SourceSide side)
        {
            var data = new byte[] { 3, 4, 5 };
            var sourceFileName = Path.Combine(_DataDir, string.Concat(
                "11.", side.ToString().ToLowerInvariant()));

            File.WriteAllBytes(sourceFileName, data);
            try
            {
                var storage = new DiskStorage(_Options);
                var (_, readiness) = await storage.LoadDiffAsync(11);

                readiness.Should().Be(DifferenceReadiness.NotReady);
            }
            finally
            {
                File.Delete(sourceFileName);
            }
        }
    }
}

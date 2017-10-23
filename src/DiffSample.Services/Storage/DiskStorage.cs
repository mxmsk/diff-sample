using System;
using System.IO;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using DiffSample.Abstractions.Configuration;
using DiffSample.Abstractions.Storage;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DiffSample.Services.Storage
{
    /// <summary>
    /// Implements persistent storage by using directories and files on disk.
    /// </summary>
    public class DiskStorage : IStorage
    {
        private readonly string _dataDir;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DiskStorage"/> class.
        /// </summary>
        /// <param name="options">The options containing setup info.</param>
        /// <remarks>
        /// Disk storage is used for demo purposes in order to work without
        /// any database dependency.
        /// </remarks>
        public DiskStorage(IOptions<DifferenceOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _dataDir = options.Value.DataDir;
            Directory.CreateDirectory(_dataDir);
        }

        /// <summary>
        /// Saves the content of the source in the storage.
        /// </summary>
        /// <param name="content">The content to save.</param>
        /// <returns>The task that saves the content.</returns>
        public async Task SaveSourceAsync(DiffBag<SourceContent> content)
        {
            var fileName = GetSourceFileName(content.DiffId, content.Data.SourceSide);
            var bytes = content.Data.Data;

            await File.WriteAllBytesAsync(fileName, bytes);
        }

        /// <summary>
        /// Saves the content of the diff in the storage.
        /// </summary>
        /// <param name="content">The content to save.</param>
        /// <returns>The task that saves the content.</returns>
        public async Task SaveDiffAsync(DiffBag<DifferenceContent> content)
        {
            var fileName = GetDiffFileName(content.DiffId);
            var json = JsonConvert.SerializeObject(content.Data);

            await File.WriteAllTextAsync(fileName, json);
        }

        /// <summary>
        /// Loads content of source by diff identifier and side.
        /// </summary>
        /// <param name="diffId">Identifier of diff which the source belongs to.</param>
        /// <param name="side">The side a content had come from.</param>
        /// <returns>The task that loads content of the source.</returns>
        public async Task<(SourceContent, bool)> LoadSourceAsync(int diffId, SourceSide side)
        {
            byte[] bytes;

            var fileName = GetSourceFileName(diffId, side);
            try
            {
                bytes = await File.ReadAllBytesAsync(fileName);
            }
            catch (FileNotFoundException)
            {
                return (null, false);
            }

            var content = new SourceContent
            {
                SourceSide = side,
                Data = bytes
            };

            return (content, true);
        }

        /// <summary>
        /// Loads diff by its identifier.
        /// </summary>
        /// <param name="diffId">Identifier of diff to load.</param>
        /// <returns>The task that finds the diff; the readiness shows availability of diff.</returns>
        public async Task<(DifferenceContent, DifferenceReadiness)> LoadDiffAsync(int diffId)
        {
            string json;

            var fileName = GetDiffFileName(diffId);
            try
            {
                json = await File.ReadAllTextAsync(fileName);
            }
            catch (FileNotFoundException)
            {
                // Check if any source exists.
                // If it is, treat that diff just not ready.
                if (File.Exists(GetSourceFileName(diffId, SourceSide.Left))
                    || File.Exists(GetSourceFileName(diffId, SourceSide.Right)))
                {
                    return (null, DifferenceReadiness.NotReady);
                }
                return (null, DifferenceReadiness.NotFound);
            }

            var content = JsonConvert.DeserializeObject<DifferenceContent>(json);
            return (content, DifferenceReadiness.Ready);
        }

        private string GetSourceFileName(int diffId, SourceSide side)
        {
            var fileName = string.Concat(diffId.ToString(), ".", side.ToString().ToLowerInvariant());
            return Path.Combine(_dataDir, fileName);
        }

        private string GetDiffFileName(int diffId)
        {
            var fileName = string.Concat(diffId.ToString(), ".diff");
            return Path.Combine(_dataDir, fileName);
        }
    }
}

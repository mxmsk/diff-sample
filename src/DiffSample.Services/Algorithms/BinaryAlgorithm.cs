using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiffSample.Abstractions;

namespace DiffSample.Services.Algorithms
{
    /// <summary>
    /// Implements algorithm for binary comparison.
    /// </summary>
    public class BinaryAlgorithm : IDifferenceAlgorithm
    {
        /// <summary>
        /// Compares sources and generates the difference.
        /// </summary>
        /// <param name="contents">Contents to be compared.</param>
        /// <returns>The task that generates the difference.</returns>
        public Task<DifferenceContent> GetDiffAsync(IEnumerable<SourceContent> contents)
        {
            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }

            var left = contents.FirstOrDefault(c => c.SourceSide == SourceSide.Left);
            if (left == null)
            {
                throw new ArgumentException("The left part is missed", nameof(contents));
            }

            var right = contents.FirstOrDefault(c => c.SourceSide == SourceSide.Right);
            if (right == null)
            {
                throw new ArgumentException("The right part is missed", nameof(contents));
            }

            return GetBinaryDiffAsync(left.Data, right.Data);
        }

        private Task<DifferenceContent> GetBinaryDiffAsync(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return Task.FromResult(new DifferenceContent { Type = DifferenceType.SizeDiffers });
            }
            if (left.Length == 0)
            {
                return Task.FromResult(new DifferenceContent { Type = DifferenceType.Equal });
            }

            var details = WalkForDiffDetails(left, right).ToList();

            return Task.FromResult(new DifferenceContent
            {
                Type = details.Count > 0 ? DifferenceType.Detailed : DifferenceType.Equal,
                Details = details,
            });
        }

        private IEnumerable<DifferenceDetail> WalkForDiffDetails(byte[] left, byte[] right)
        {
            // WalkForDiffDetails compares arrays byte-to-byte.
            // When difference is met we begin new DifferenceDetail saving offsets in it.
            // Then difference consider finished when:
            // 1) Left byte is equal to right where detail have started.
            // 2) Right byte is equal to left where detail have started.
            // 3) Left byte is equal to right.
            // The algorithm is simple enough and doesn't play good because it can't go
            // back. We have to switch to more sophisticated method in a future. For example:
            // https://www.codeproject.com/Articles/6943/A-Generic-Reusable-Diff-Algorithm-in-C-II
        
            var l = 0;
            var r = 0;
            DifferenceDetail d = null;

            while (l < left.Length && r < right.Length)
            {
                if (d != null)
                {
                    var diffFound = false;

                    if (left[l] == right[d.RightOffset])
                    {
                        // Change is in left but not in right.
                        d.LeftLength = l - d.LeftOffset;
                        r = d.RightOffset;

                        diffFound = true;
                    }
                    else if (right[r] == left[d.LeftOffset])
                    {
                        // Change is in right but not in left.
                        d.RightLength = r - d.RightOffset;
                        l = d.LeftOffset;

                        diffFound = true;
                    }
                    else if (left[l] == right[r])
                    {
                        // Both sides have equal difference of change.
                        d.LeftLength = l - d.LeftOffset;
                        d.RightLength = r - d.RightOffset;

                        diffFound = true;
                    }

                    if (diffFound)
                    {
                        yield return d;
                        d = null;
                    }
                }
                else if (left[l] != right[r])
                {
                    // We meet the difference after equality.
                    // Start new detail at positions.
                    d = new DifferenceDetail
                    {
                        LeftOffset = l,
                        RightOffset = r,
                    };
                }

                l++;
                r++;
            }

            // Check whether change stretches to the end.
            if (d != null)
            {
                d.LeftLength = left.Length - d.LeftOffset;
                d.RightLength = right.Length - d.RightOffset;
                yield return d;
            }
        }
    }
}
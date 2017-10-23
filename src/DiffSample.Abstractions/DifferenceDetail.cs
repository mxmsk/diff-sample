namespace DiffSample.Abstractions
{
    /// <summary>
    /// Describes a single block of difference.
    /// </summary>
    public class DifferenceDetail
    {
        /// <summary>
        /// Gets or sets the offset where block starts in the left source.
        /// </summary>
        public int LeftOffset { get; set; }

        /// <summary>
        /// Gets or sets the length of difference in the left source.
        /// </summary>
        public int LeftLength { get; set; }

        /// <summary>
        /// Gets or sets the offset where block starts in the right source.
        /// </summary>
        public int RightOffset { get; set; }

        /// <summary>
        /// Gets or sets the length of difference in the right source.
        /// </summary>
        public int RightLength { get; set; }
    }
}

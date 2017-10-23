namespace DiffSample.Abstractions
{
    /// <summary>
    /// Represents a content to be compared for a diff.
    /// </summary>
    public class SourceContent
    {
        /// <summary>
        /// Gets or sets the data of content.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the side of content's source.
        /// </summary>
        public SourceSide SourceSide { get; set; }
    }
}

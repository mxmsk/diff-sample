namespace DiffSample.Model
{
    /// <summary>
    /// Describes the request for uploading content for a difference.
    /// </summary>
    public class SourceContentRequest
    {
        /// <summary>
        /// Gets or sets Base-64 encoded data to compare for difference.
        /// </summary>
        public string Data { get; set; }
    }
}

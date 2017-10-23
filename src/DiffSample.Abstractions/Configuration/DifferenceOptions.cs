using System.IO;

namespace DiffSample.Abstractions.Configuration
{
    /// <summary>
    /// Provides configuration for the difference services.
    /// </summary>
    public class DifferenceOptions
    {
        /// <summary>
        /// Gets or sets directory to store pipeline data on disk.
        /// </summary>
        /// <remarks>
        /// We init it with some arbitrary path, being sure that setup code
        /// configure it with a proper value.
        /// </remarks>
        public string DataDir { get; set; } = Path.GetTempPath();
    }
}

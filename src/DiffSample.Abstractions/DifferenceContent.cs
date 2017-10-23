using System.Collections.Generic;
using System.Linq;

namespace DiffSample.Abstractions
{
    /// <summary>
    /// Represents a result of source content comparison.
    /// </summary>
    public class DifferenceContent
    {
        /// <summary>
        /// Gets or sets the type of difference.
        /// </summary>
        public DifferenceType Type { get; set; }

        /// <summary>
        /// Gets or sets the difference details for type <see cref="DifferenceType.Detailed"/>.
        /// </summary>
        public IEnumerable<DifferenceDetail> Details { get; set; } = Enumerable.Empty<DifferenceDetail>();
    }
}

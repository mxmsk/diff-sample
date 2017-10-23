namespace DiffSample.Abstractions
{
    /// <summary>
    /// Defines types of difference.
    /// </summary>
    public enum DifferenceType
    {
        /// <summary>
        /// Source contents are identical.
        /// </summary>
        Equal,

        /// <summary>
        /// Source contents has different size.
        /// </summary>
        SizeDiffers,

        /// <summary>
        /// Detailed info provided in a difference content.
        /// </summary>
        Detailed
    }
}

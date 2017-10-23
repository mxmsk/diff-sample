namespace DiffSample.Abstractions
{
    /// <summary>
    /// Defines availability of difference.
    /// </summary>
    public enum DifferenceReadiness
    {
        /// <summary>
        /// There is not enough info to compute the difference.
        /// </summary>
        NotFound,

        /// <summary>
        /// The difference is in process.
        /// </summary>
        NotReady,

        /// <summary>
        /// The difference is ready.
        /// </summary>
        Ready,
    }
}

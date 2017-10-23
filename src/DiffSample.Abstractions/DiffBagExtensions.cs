namespace DiffSample.Abstractions
{
    /// <summary>
    /// Provides extension methods for wrapping data into <see cref="T:DiffBag`1"/> class.
    /// </summary>
    public static class DiffBagExtensions
    {
        /// <summary>
        /// Wraps a given data into diff bag.
        /// </summary>
        /// <typeparam name="T">The type of data to wrap.</typeparam>
        /// <param name="data">The data to wrap.</param>
        /// <param name="diffId">The identifier of diff which data belongs to.</param>
        /// <returns>The given data with the given diff id.</returns>
        public static DiffBag<T> InDiffBag<T>(this T data, int diffId)
        {
            return new DiffBag<T>(diffId, data);
        }
    }
}

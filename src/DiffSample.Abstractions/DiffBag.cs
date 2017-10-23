using System.Collections.Generic;

namespace DiffSample.Abstractions
{
    /// <summary>
    /// Represents a data identified by diff id.
    /// </summary>
    public class DiffBag<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:DiffBag`1"/> class.
        /// </summary>
        /// <param name="diffId">The identifier of diff.</param>
        /// <param name="data">The data to be identified.</param>
        public DiffBag(int diffId, T data)
        {
            DiffId = diffId;
            Data = data;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the
        /// current <see cref="T:DiffBag`1"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:DiffBag`1"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the
        /// current <see cref="T:DiffBag`1"/>; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (obj is DiffBag<T> other)
            {
                return other.DiffId == DiffId && Equals(other.Data, Data);
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:DiffBag`1"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in
        /// hashing algorithms and data structures such as a hash table.</returns>
        public override int GetHashCode() => DiffId.GetHashCode() ^ EqualityComparer<T>.Default.GetHashCode(Data);

        /// <summary>
        /// Gets or sets the diff identifier.
        /// </summary>
        public int DiffId { get; }

        /// <summary>
        /// Gets or sets the data identified by diff id.
        /// </summary>
        public T Data { get; }
    }
}

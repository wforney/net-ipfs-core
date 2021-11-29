using Ipfs.Registry;

namespace Ipfs
{
    /// <summary>
    ///   Provides data for the unknown hashing algorithm event.
    /// </summary>
    public class UnknownHashingAlgorithmEventArgs : EventArgs
    {
        /// <summary>
        ///   The <see cref="HashingAlgorithm"/> that is defined for the
        ///   unknown hashing number.
        /// </summary>
        public HashingAlgorithm? Algorithm { get; set; }
    }
}

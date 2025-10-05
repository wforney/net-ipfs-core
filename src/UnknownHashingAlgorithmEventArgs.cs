using Ipfs.Registry;

namespace Ipfs;

/// <summary>
/// Provides data for the unknown hashing algorithm event.
/// </summary>
public class UnknownHashingAlgorithmEventArgs : EventArgs
{
    internal UnknownHashingAlgorithmEventArgs(HashingAlgorithm a) => Algorithm = a;

    /// <summary>
    /// The <see cref="HashingAlgorithm"/> that is defined for the unknown hashing number.
    /// </summary>
    public HashingAlgorithm Algorithm { get; }
}

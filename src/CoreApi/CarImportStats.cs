using Newtonsoft.Json;

namespace Ipfs.CoreApi;

/// <summary>
/// Statistics about an imported CAR file.
/// </summary>
public record CarImportStats
{
    /// <summary>
    /// The number of blocks in the CAR file.
    /// </summary>
    [JsonProperty(nameof(BlockCount))]
    public ulong BlockCount { get; set; }

    /// <summary>
    /// The number of block bytes in the CAR file.
    /// </summary>
    [JsonProperty(nameof(BlockBytesCount))]
    public ulong BlockBytesCount { get; set; }
}

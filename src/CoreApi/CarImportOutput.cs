using Newtonsoft.Json;

namespace Ipfs.CoreApi;

/// <summary>
/// CarImportOutput is the output type of the 'dag import' commands
/// </summary>
/// <remarks>See also <see href="https://github.com/ipfs/kubo/blob/f5b855550ca73acf5dd3a2001e2a7192cab7c249/core/commands/dag/dag.go#L63"/></remarks>
public class CarImportOutput
{
    /// <summary>
    /// Root is the metadata for a root pinning response
    /// </summary>
    [JsonProperty(nameof(Root), NullValueHandling = NullValueHandling.Ignore)]
    public CarImportRootMeta? Root { get; set; }

    /// <summary>
    /// Stats contains statistics about the imported CAR file, if requested.
    /// </summary>
    [JsonProperty(nameof(Stats), NullValueHandling = NullValueHandling.Ignore)]
    public CarImportStats? Stats { get; set; }

}

namespace Ipfs.CoreApi;

/// <summary>
/// RootMeta is the metadata for a root pinning response
/// </summary>
public record CarImportRootMeta
{
    /// <summary>
    /// The CID of the root of the imported DAG.
    /// </summary>
    public required DagCid Cid { get; set; }

    /// <summary>
    /// The error message if pinning failed
    /// </summary>
    public string? PinErrorMsg { get; set; }
}

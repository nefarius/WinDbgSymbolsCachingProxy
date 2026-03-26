namespace HarvestingAgent;

public sealed class DeleteOverlapWarningGroup
{
    public required string Path { get; init; }
    public required bool Recursive { get; init; }
    public required List<DeleteOverlapWarningServerEntry> Servers { get; init; }
}

public sealed class DeleteOverlapWarningServerEntry
{
    public required string Display { get; init; }
    public required bool DeleteEnabled { get; init; }
}


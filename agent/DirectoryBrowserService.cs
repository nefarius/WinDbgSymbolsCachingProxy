namespace HarvestingAgent;

public sealed class DirectoryBrowserNode
{
    public required string FullPath { get; init; }

    public required string DisplayName { get; init; }

    public bool HasChildren { get; init; }

    public string? Error { get; init; }
}

public sealed class DirectoryBrowserListResult
{
    public required string CurrentPath { get; init; }

    public required IReadOnlyList<DirectoryBrowserNode> Items { get; init; }

    public string? Error { get; init; }
}

public sealed class DirectoryBrowserService
{
    public DirectoryBrowserListResult ListRoots()
    {
        List<DirectoryBrowserNode> items = [];

        try
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives().OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
            {
                string path = PathNormalization.NormalizeDirectoryPath(drive.Name);
                bool hasChildren = false;
                try
                {
                    hasChildren = Directory.EnumerateDirectories(path).Take(1).Any();
                }
                catch
                {
                    // keep false
                }

                items.Add(new DirectoryBrowserNode
                {
                    FullPath = path,
                    DisplayName = path,
                    HasChildren = hasChildren
                });
            }
        }
        catch
        {
            string root = PathNormalization.NormalizeDirectoryPath(Path.GetPathRoot(Environment.CurrentDirectory));
            items.Add(new DirectoryBrowserNode
            {
                FullPath = root,
                DisplayName = root,
                HasChildren = true
            });
        }

        return new DirectoryBrowserListResult
        {
            CurrentPath = string.Empty,
            Items = items
        };
    }

    public DirectoryBrowserListResult ListChildren(string path)
    {
        string normalized = PathNormalization.NormalizeDirectoryPath(path);
        List<DirectoryBrowserNode> items = [];

        try
        {
            foreach (string directory in Directory.EnumerateDirectories(normalized).OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                string fullPath = PathNormalization.NormalizeDirectoryPath(directory);
                string name = Path.GetFileName(fullPath);
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = fullPath;
                }

                bool hasChildren = false;
                string? error = null;
                try
                {
                    hasChildren = Directory.EnumerateDirectories(fullPath).Take(1).Any();
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }

                items.Add(new DirectoryBrowserNode
                {
                    FullPath = fullPath,
                    DisplayName = name,
                    HasChildren = hasChildren,
                    Error = error
                });
            }

            return new DirectoryBrowserListResult
            {
                CurrentPath = normalized,
                Items = items
            };
        }
        catch (Exception ex)
        {
            return new DirectoryBrowserListResult
            {
                CurrentPath = normalized,
                Items = [],
                Error = ex.Message
            };
        }
    }

    public string? GetParentPath(string path)
    {
        string normalized = PathNormalization.NormalizeDirectoryPath(path);
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        string? parent = Directory.GetParent(normalized)?.FullName;
        return string.IsNullOrEmpty(parent) ? null : PathNormalization.NormalizeDirectoryPath(parent);
    }
}

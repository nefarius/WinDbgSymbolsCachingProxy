using System.Security;

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
    /// <summary>
    /// Builds a list of filesystem root directories (drive roots) as DirectoryBrowserNode entries.
    /// </summary>
    /// <returns>
    /// A DirectoryBrowserListResult with CurrentPath set to an empty string and Items containing a node for each discovered root drive. 
    /// Each node's HasChildren is true when at least one immediate subdirectory exists; per-drive errors leave HasChildren false and do not abort listing.
    /// If enumerating drives fails, Items contains a single node for the current working directory's root with HasChildren set to true.
    /// </returns>
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

    /// <summary>
    /// Lists immediate subdirectories of the given path as directory nodes for browsing.
    /// </summary>
    /// <param name="path">The directory path to list; the path is normalized before enumeration.</param>
    /// <returns>
    /// A DirectoryBrowserListResult whose CurrentPath is the normalized input and Items contains a node for each immediate child directory.
    /// If enumerating the input path fails, Items will be empty and the result's Error will contain the exception message.
    /// Each DirectoryBrowserNode's Error, if present, contains the exception message that occurred while probing that child's children.
    /// </returns>
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
                    Console.Error.WriteLine(
                        $"[DirectoryBrowserService] Failed to probe children for '{fullPath}': {ex}");
                    error = ex is UnauthorizedAccessException or SecurityException
                        ? "Access denied"
                        : "Unable to read directory";
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
            Console.Error.WriteLine(
                $"[DirectoryBrowserService] Failed to list children for '{normalized}': {ex}");
            return new DirectoryBrowserListResult
            {
                CurrentPath = normalized,
                Items = [],
                Error = ex is UnauthorizedAccessException or SecurityException
                    ? "Access denied"
                    : "Unable to read directory"
            };
        }
    }

    /// <summary>
    /// Get the normalized parent directory path for the given path.
    /// </summary>
    /// <returns>The normalized parent directory path, or `null` if the input is empty or no parent exists.</returns>
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

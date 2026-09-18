using MyEngine.Diagnostics;

namespace MyEngine.Assets;

/// <summary>
/// Сканирует папку Assets и строит дерево файлов.
/// Не загружает содержимое — только структуру. Ленивая загрузка
/// превью делает AssetBrowserWindow.
/// </summary>
public sealed class AssetDatabase
{
    public AssetEntry Root { get; private set; } = new();
    public string RootPath { get; private set; } = "";
    public bool Scanned { get; private set; }

    /// <summary>Сканировать папку. Вызывать один раз при старте + по F5.</summary>
    public void Scan(string rootPath)
    {
        RootPath = Path.GetFullPath(rootPath);

        if (!Directory.Exists(RootPath))
        {
            Log.Warn("AssetDatabase", $"Directory not found: {RootPath}");
            Root = new AssetEntry { Name = "(not found)", IsDirectory = true };
            Scanned = false;
            return;
        }

        Root = new AssetEntry
        {
            Name = "Assets",
            FullPath = RootPath,
            RelativePath = "",
            Kind = AssetKind.Folder,
            IsDirectory = true
        };

        ScanFolder(Root);
        Scanned = true;

        Log.Info("AssetDatabase", $"Scanned: {CountFiles(Root)} files");
    }

    /// <summary>Пересчитать. Используется по кнопке Refresh.</summary>
    public void Refresh() => Scan(RootPath);

    private void ScanFolder(AssetEntry parent)
    {
        try
        {
            foreach (var dir in Directory.GetDirectories(parent.FullPath))
            {
                var entry = MakeEntry(dir, isDirectory: true);
                parent.Children.Add(entry);
                ScanFolder(entry);
            }

            foreach (var file in Directory.GetFiles(parent.FullPath))
            {
                parent.Children.Add(MakeEntry(file, isDirectory: false));
            }

            // Сортировка: папки сверху, потом файлы по имени
            parent.Children.Sort((a, b) =>
            {
                if (a.IsDirectory != b.IsDirectory)
                    return a.IsDirectory ? -1 : 1;
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
        catch (Exception ex)
        {
            Log.Error("AssetDatabase", $"Failed to scan {parent.FullPath}: {ex.Message}");
        }
    }

    private AssetEntry MakeEntry(string fullPath, bool isDirectory)
    {
        var relative = Path.GetRelativePath(RootPath, fullPath);
        long size = 0;
        if (!isDirectory)
        {
            try { size = new FileInfo(fullPath).Length; } catch { }
        }

        return new AssetEntry
        {
            Name = Path.GetFileName(fullPath),
            FullPath = fullPath,
            RelativePath = relative,
            Kind = AssetEntry.DetectKind(fullPath, isDirectory),
            IsDirectory = isDirectory,
            SizeBytes = size
        };
    }

    private static int CountFiles(AssetEntry entry)
    {
        if (!entry.IsDirectory) return 1;
        int n = 0;
        foreach (var child in entry.Children) n += CountFiles(child);
        return n;
    }
}
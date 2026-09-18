using ImGuiNET;
using System.Numerics;

namespace MyEngine.Assets;

/// <summary>
/// ImGui-окно для просмотра ассетов. Дерево слева, превью справа.
/// Открывается по клавише (обычно F4), работает только в DEBUG.
/// </summary>
public sealed class AssetBrowserWindow
{
    private readonly AssetDatabase _db;
    private readonly AssetPreview _preview;

    private AssetEntry? _selected;
    private string _searchFilter = "";

    public bool Visible { get; set; } = false;

    public AssetBrowserWindow(AssetDatabase db, AssetPreview preview)
    {
        _db = db;
        _preview = preview;
    }

    public void Toggle() => Visible = !Visible;

    public void Draw()
    {
        if (!Visible) return;

        var io = ImGui.GetIO();
        var size = new Vector2(900, 600);
        ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(
            (io.DisplaySize.X - size.X) * 0.5f,
            (io.DisplaySize.Y - size.Y) * 0.5f), ImGuiCond.FirstUseEver);

        ImGui.Begin("asset browser", ImGuiWindowFlags.NoSavedSettings);

        // Тулбар
        if (ImGui.Button("Refresh")) _db.Refresh();
        ImGui.SameLine();
        ImGui.Text($"Root: {_db.RootPath}");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200);
        ImGui.InputTextWithHint("##search", "Search...", ref _searchFilter, 128);

        ImGui.Separator();

        // Две колонки: дерево слева, превью справа
        float treeWidth = 320f;
        float avail = ImGui.GetContentRegionAvail().X;

        ImGui.BeginChild("tree", new Vector2(treeWidth, 0), ImGuiChildFlags.Border);
        DrawTree(_db.Root, 0);
        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("preview", new Vector2(avail - treeWidth - 8, 0), ImGuiChildFlags.Border);
        DrawPreview();
        ImGui.EndChild();

        ImGui.End();
    }

    // ============================================================
    // Дерево
    // ============================================================

    private void DrawTree(AssetEntry entry, int depth)
    {
        if (entry.IsDirectory)
        {
            var flags = entry == _db.Root
                ? ImGuiTreeNodeFlags.DefaultOpen
                : ImGuiTreeNodeFlags.OpenOnArrow;

            bool open = ImGui.TreeNodeEx(entry.Name + "##" + entry.FullPath, flags);

            if (ImGui.IsItemClicked())
                _selected = entry;

            if (open)
            {
                foreach (var child in entry.Children)
                {
                    if (!string.IsNullOrEmpty(_searchFilter) &&
                        !child.Name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    DrawTree(child, depth + 1);
                }
                ImGui.TreePop();
            }
        }
        else
        {
            // Файл — с иконкой по типу
            string label = GetIcon(entry.Kind) + " " + entry.Name;

            bool isSelected = entry == _selected;
            if (ImGui.Selectable(label, isSelected))
                _selected = entry;

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(entry.RelativePath);
                ImGui.Text($"Size: {entry.SizeBytes} bytes");
                ImGui.Text($"Kind: {entry.Kind}");
                ImGui.EndTooltip();
            }
        }
    }

    private static string GetIcon(AssetKind kind) => kind switch
    {
        AssetKind.Texture => "IMG",
        AssetKind.Json => "JSON",
        AssetKind.Text => "TXT",
        AssetKind.Font => "TTF",
        AssetKind.Audio => "WAV",
        AssetKind.Folder => "DIR",
        _ => "???"
    };

    // ============================================================
    // Превью
    // ============================================================

    private void DrawPreview()
    {
        if (_selected == null)
        {
            ImGui.TextDisabled("Выбери файл слева");
            return;
        }

        if (_selected.IsDirectory)
        {
            ImGui.Text($"Папка: {_selected.Name}");
            ImGui.Text($"Файлов внутри: {_selected.Children.Count}");
            return;
        }

        ImGui.Text(_selected.RelativePath);
        ImGui.Separator();

        switch (_selected.Kind)
        {
            case AssetKind.Texture:
                DrawTexturePreview(_selected.FullPath);
                break;

            case AssetKind.Json:
            case AssetKind.Text:
                DrawTextPreview(_selected.FullPath);
                break;

            case AssetKind.Font:
                ImGui.Text("Шрифт (превью не поддерживается)");
                ImGui.Text($"Размер: {_selected.SizeBytes} байт");
                break;

            case AssetKind.Audio:
                ImGui.Text("Аудио (превью не поддерживается)");
                ImGui.Text($"Размер: {_selected.SizeBytes} байт");
                break;

            default:
                ImGui.Text("Неизвестный формат");
                break;
        }
    }

    private void DrawTexturePreview(string path)
    {
        var tex = _preview.GetTexture(path);
        if (tex == null)
        {
            ImGui.TextColored(new Vector4(1, 0.4f, 0.4f, 1), "Не удалось загрузить");
            return;
        }

        ImGui.Text($"Размер: {tex.Width} x {tex.Height}");

        // Масштаб вписываем в доступную область
        var avail = ImGui.GetContentRegionAvail();
        float maxW = avail.X - 10;
        float maxH = avail.Y - 30;

        float scale = System.Math.Min(
            maxW / tex.Width,
            maxH / tex.Height);
        scale = System.Math.Min(scale, 4f);   // не растягивать больше ×4
        scale = System.Math.Max(scale, 0.1f); // и не сжимать в ничто

        var drawSize = new Vector2(tex.Width * scale, tex.Height * scale);

        // Шахматный фон — для прозрачности
        var pos = ImGui.GetCursorScreenPos();
        var dl = ImGui.GetWindowDrawList();
        DrawCheckerboard(dl, pos, drawSize);

        // Картинка
        var texId = (IntPtr)tex.Handle;
        ImGui.Image(texId, drawSize, new Vector2(0, 1), new Vector2(1, 0));
    }

    private static void DrawCheckerboard(ImDrawListPtr dl, Vector2 pos, Vector2 size)
    {
        const float cell = 16f;
        for (float y = 0; y < size.Y; y += cell)
        {
            for (float x = 0; x < size.X; x += cell)
            {
                bool dark = ((int)(x / cell) + (int)(y / cell)) % 2 == 0;
                uint col = dark
                    ? ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.3f, 0.3f, 1f))
                    : ImGui.ColorConvertFloat4ToU32(new Vector4(0.4f, 0.4f, 0.4f, 1f));

                var p0 = new Vector2(pos.X + x, pos.Y + y);
                var p1 = new Vector2(
                    System.Math.Min(pos.X + x + cell, pos.X + size.X),
                    System.Math.Min(pos.Y + y + cell, pos.Y + size.Y));
                dl.AddRectFilled(p0, p1, col);
            }
        }
    }

    private void DrawTextPreview(string path)
    {
        var text = _preview.GetText(path);
        if (text == null)
        {
            ImGui.TextColored(new Vector4(1, 0.4f, 0.4f, 1), "Не удалось прочитать");
            return;
        }

        ImGui.BeginChild("text_scroll", new Vector2(0, 0), ImGuiChildFlags.Border);
        ImGui.TextUnformatted(text);
        ImGui.EndChild();
    }
}
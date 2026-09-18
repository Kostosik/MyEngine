#if DEBUG
using MyEngine.Assets;
using MyEngine.InputEngine;
using Silk.NET.Input;
using Silk.NET.OpenGL;

namespace MyEngine.Diagnostics.Tools;

public sealed class AssetBrowserTool : IDebugTool, IDisposable
{
    private readonly AssetDatabase _db;
    private readonly AssetPreview _preview;
    private readonly AssetBrowserWindow _window;

    public string Name => "AssetBrowser";
    public bool Visible { get => _window.Visible; set => _window.Visible = value; }
    public AssetBrowserWindow Window => _window;

    public AssetBrowserTool(GL gl, string assetsPath)
    {
        _db = new AssetDatabase();
        _db.Scan(assetsPath);
        _preview = new AssetPreview(gl);
        _window = new AssetBrowserWindow(_db, _preview);
    }

    public void ProcessHotkeys(Input input)
    {
        if (input.ConsumeDebugPressed(DebugAction.ToggleAssetBrowser))
            _window.Toggle();
    }

    public void Draw() => _window.Draw();

    public void Dispose() => _preview.Dispose();
}
#endif
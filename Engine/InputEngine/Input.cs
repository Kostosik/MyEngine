using Silk.NET.Input;
using System.Numerics;

namespace MyEngine.InputEngine;

public sealed class Input
{
    // Сколько кадров рендера нажатие считается "свежим".
    // 8 кадров при 140 FPS ≈ 55 мс — с запасом больше одного фикс. апдейта (16.6 мс)
    private const int OneShotBufferFrames = 8;

    public Bindings Bindings { get; } = new();

    private readonly IKeyboard? _keyboard;
    private readonly IMouse? _mouse;

    private readonly HashSet<Key> _down = new();
    private readonly Dictionary<Key, int> _pressedFrame = new();
    private readonly Dictionary<Key, int> _releasedFrame = new();

    private readonly HashSet<MouseButton> _mouseDown = new();
    private readonly Dictionary<MouseButton, int> _mousePressedFrame = new();

    private int _frameCounter;

    public Vector2 MousePosition { get; private set; }

    public bool IsDebugDown(DebugAction a) => IsDown(Bindings.Get(a));
    public bool WasDebugPressed(DebugAction a) => WasPressed(Bindings.Get(a));
    public bool ConsumeDebugPressed(DebugAction a) => ConsumePressed(Bindings.Get(a));

    public Input(IInputContext ctx)
    {
        _keyboard = ctx.Keyboards.Count > 0 ? ctx.Keyboards[0] : null;

        _mouse = ctx.Mice.Count > 0 ? ctx.Mice[0] : null;
        if (_mouse != null)
        {
            _mouse.MouseDown += (_, btn) =>
            {
                if (_mouseDown.Add(btn))
                    _mousePressedFrame[btn] = _frameCounter;
            };
            _mouse.MouseUp += (_, btn) => _mouseDown.Remove(btn);
        }

        if (_keyboard != null)
        {
            _keyboard.KeyDown += (_, key, _) =>
            {
                if (_down.Add(key))
                    _pressedFrame[key] = _frameCounter;
            };
            _keyboard.KeyUp += (_, key, _) =>
            {
                if (_down.Remove(key))
                    _releasedFrame[key] = _frameCounter;
            };
        }
    }

    public bool IsDown(Key k) => _down.Contains(k);

    public bool WasPressed(Key k)
    {
        if (!_pressedFrame.TryGetValue(k, out int f)) return false;
        return (_frameCounter - f) <= OneShotBufferFrames;
    }

    public bool WasReleased(Key k)
    {
        if (!_releasedFrame.TryGetValue(k, out int f)) return false;
        return (_frameCounter - f) <= OneShotBufferFrames;
    }

    // Actions
    public bool IsActionDown(GameAction a) => IsDown(Bindings.Get(a));
    public bool WasActionPressed(GameAction a) => WasPressed(Bindings.Get(a));
    public bool WasActionReleased(GameAction a) => WasReleased(Bindings.Get(a));

    public void BeginFrame()
    {
        if (_mouse != null)
            MousePosition = new Vector2(_mouse.Position.X, _mouse.Position.Y);
    }

    public void EndFrame()
    {
        _frameCounter++;

        // Раз в 256 кадров подчищаем старые записи — иначе словари растут
        if ((_frameCounter & 0xFF) == 0)
        {
            PruneOld(_pressedFrame);
            PruneOld(_releasedFrame);
        }
    }

    private void PruneOld(Dictionary<Key, int> dict)
    {
        var toRemove = new List<Key>();
        foreach (var kv in dict)
        {
            if (_frameCounter - kv.Value > OneShotBufferFrames * 4)
                toRemove.Add(kv.Key);
        }
        foreach (var k in toRemove) dict.Remove(k);
    }

    public bool ConsumeActionPressed(GameAction a) => ConsumePressed(Bindings.Get(a));

    public bool ConsumePressed(Key k)
    {
        if (!WasPressed(k)) return false;
        _pressedFrame.Remove(k);
        return true;
    }

    public bool IsMouseDown(MouseButton btn) => _mouseDown.Contains(btn);

    public bool WasMousePressed(MouseButton btn)
    {
        if (!_mousePressedFrame.TryGetValue(btn, out int f)) return false;
        return (_frameCounter - f) <= OneShotBufferFrames;
    }

    public bool ConsumeMousePressed(MouseButton btn)
    {
        if (!WasMousePressed(btn)) return false;
        _mousePressedFrame.Remove(btn);
        return true;
    }
}
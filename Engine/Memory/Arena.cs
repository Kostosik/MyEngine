using MyEngine.Diagnostics;
using System.Runtime.InteropServices;

namespace MyEngine.Memory;

/// <summary>
/// Простой bump allocator: выделяет куски из заранее заданного буфера.
/// Никакой деаллокации отдельных блоков — Reset() освобождает всё сразу.
/// Не потокобезопасен: по одному на поток.
/// </summary>
public sealed class Arena
{
    private byte[] _buffer;
    private int _offset;

    public Arena(int initialSizeBytes = 1 * 1024 * 1024)
    {
        _buffer = new byte[initialSizeBytes];
    }

    public int Capacity => _buffer.Length;
    public int Used => _offset;
    public int Free => _buffer.Length - _offset;

    public Span<T> Alloc<T>(int count = 1) where T : unmanaged
    {
        if (count <= 0) return Span<T>.Empty;

        int elementSize = Marshal.SizeOf<T>();
        int align = elementSize >= 8 ? 8 : elementSize;
        int bytes = elementSize * count;

        int alignedOffset = (_offset + align - 1) & ~(align - 1);
        int newOffset = alignedOffset + bytes;

        if (newOffset > _buffer.Length)
            Grow(newOffset);

        _offset = newOffset;
        var byteSpan = _buffer.AsSpan(alignedOffset, bytes);
        return MemoryMarshal.Cast<byte, T>(byteSpan);
    }

    public Span<byte> AllocBytes(int bytes, int align = 8)
    {
        if (bytes <= 0) return Span<byte>.Empty;

        int alignedOffset = (_offset + align - 1) & ~(align - 1);
        int newOffset = alignedOffset + bytes;

        if (newOffset > _buffer.Length)
            Grow(newOffset);

        _offset = newOffset;
        return _buffer.AsSpan(alignedOffset, bytes);
    }

    public void Reset() => _offset = 0;

    private void Grow(int needed)
    {
        int newSize = _buffer.Length;
        while (newSize < needed) newSize *= 2;
        Array.Resize(ref _buffer, newSize);
        Log.Info("[Arena]", $" Grew to {newSize / 1024} KB");
    }
}
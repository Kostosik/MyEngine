using System.Numerics;

namespace MyEngine.WorldEngine;

/// <summary>
/// Кэш чанка, в котором лежит сущность. Обновляется ChunkSystem
/// раз в N кадров. Позволяет TickScheduler быстро проверять
/// загруженность без вычисления координат.
/// </summary>
public sealed class ChunkRef
{
    public int ChunkX;
    public int ChunkY;
    public bool Loaded;
}

/// <summary>
/// Координата чанка в мире. Не путать с мировыми координатами —
/// это индекс чанка (0,0), (1,0), (-1,2) и т.д.
/// </summary>
public readonly struct ChunkCoord : IEquatable<ChunkCoord>
{
    public readonly int X;
    public readonly int Y;

    public ChunkCoord(int x, int y) { X = x; Y = y; }

    public bool Equals(ChunkCoord other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is ChunkCoord c && Equals(c);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"Chunk({X},{Y})";

    public static bool operator ==(ChunkCoord a, ChunkCoord b) => a.Equals(b);
    public static bool operator !=(ChunkCoord a, ChunkCoord b) => !a.Equals(b);
}
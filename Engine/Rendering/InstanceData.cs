using System.Numerics;
using System.Runtime.InteropServices;

namespace MyEngine.Rendering;

/// <summary>
/// Данные одного экземпляра спрайта для instanced rendering.
/// Одна структура на спрайт — передаётся как per-instance атрибут.
///
/// Выравнивание: sizeof должно быть кратно 16 для оптимальной
/// передачи в GPU. У нас 64 байта — идеально.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct InstanceData
{
    public Vector2 Position;    // 8 байт
    public Vector2 Size;        // 8 байт
    public Vector4 Color;       // 16 байт
    public Vector2 UVMin;       // 8 байт
    public Vector2 UVMax;       // 8 байт
    public float Rotation;      // 4 байта
    public float _pad0;         // 4 байта padding (для кратности 16)
    public Vector2 _pad1;       // 8 байт padding

    // Итого: 64 байта на экземпляр
}
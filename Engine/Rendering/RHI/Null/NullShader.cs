using System.Numerics;

namespace MyEngine.Rendering.RHI.Null;

/// <summary>
/// Шейдер-заглушка. Все Set* методы — no-op.
/// </summary>
internal sealed class NullShader : IShader
{
    public void Use() { }
    public void SetInt(string name, int value) { }
    public void SetFloat(string name, float value) { }
    public void SetVector2(string name, Vector2 v) { }
    public void SetVector3(string name, Vector3 v) { }
    public void SetVector4(string name, Vector4 v) { }
    public void SetMatrix4(string name, Matrix4x4 m) { }
    public void Dispose() { }
}
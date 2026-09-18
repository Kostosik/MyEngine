using System.Numerics;

namespace MyEngine.Rendering.RHI;

/// <summary>
/// Абстракция шейдера. Реализации: GLShader, VulkanShader, NullShader.
/// </summary>
public interface IShader : IDisposable
{
    /// <summary>Сделать шейдер активным.</summary>
    void Use();

    // Uniform-ы. Все реализации должны поддерживать одинаковый набор.
    void SetInt(string name, int value);
    void SetFloat(string name, float value);
    void SetVector2(string name, Vector2 v);
    void SetVector3(string name, Vector3 v);
    void SetVector4(string name, Vector4 v);
    void SetMatrix4(string name, Matrix4x4 m);
}
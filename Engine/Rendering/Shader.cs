using MyEngine.Rendering.RHI;
using Silk.NET.OpenGL;
using System.Numerics;

namespace MyEngine.Rendering;

public sealed class Shader : IDisposable, IShader
{
    private readonly GL _gl;
    public uint Handle { get; }

    public Shader(GL gl, string vsSrc, string fsSrc)
    {
        _gl = gl;
        uint vs = Compile(ShaderType.VertexShader, vsSrc);
        uint fs = Compile(ShaderType.FragmentShader, fsSrc);

        Handle = _gl.CreateProgram();
        _gl.AttachShader(Handle, vs);
        _gl.AttachShader(Handle, fs);
        _gl.LinkProgram(Handle);
        _gl.GetProgram(Handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status == 0)
            throw new Exception($"Shader link error: {_gl.GetProgramInfoLog(Handle)}");

        _gl.DetachShader(Handle, vs);
        _gl.DetachShader(Handle, fs);
        _gl.DeleteShader(vs);
        _gl.DeleteShader(fs);
    }

    private uint Compile(ShaderType type, string src)
    {
        uint id = _gl.CreateShader(type);
        _gl.ShaderSource(id, src);
        _gl.CompileShader(id);
        _gl.GetShader(id, ShaderParameterName.CompileStatus, out int ok);
        if (ok == 0)
            throw new Exception($"Shader compile error ({type}): {_gl.GetShaderInfoLog(id)}");
        return id;
    }

    public void Use() => _gl.UseProgram(Handle);

    private readonly Dictionary<string, int> _uniformCache = new();

    private int GetLoc(string name)
    {
        if (_uniformCache.TryGetValue(name, out var loc)) return loc;
        loc = _gl.GetUniformLocation(Handle, name);
        _uniformCache[name] = loc;
        return loc;
    }

    public unsafe void SetMatrix4(string name, Matrix4x4 m)
    {
        int loc = GetLoc(name);
        _gl.UniformMatrix4(loc, 1, false, (float*)&m);
    }

    public void SetInt(string name, int value)
    {
        int loc = GetLoc(name);
        _gl.Uniform1(loc, value);
    }
    public void SetFloat(string name, float value)
    {
        int loc = GetLoc(name);
        _gl.Uniform1(loc, value);
    }

    public void SetVector2(string name, System.Numerics.Vector2 v)
    {
        int loc = GetLoc(name);
        _gl.Uniform2(loc, v.X, v.Y);
    }

    public void SetVector3(string name, System.Numerics.Vector3 v)
    {
        int loc = GetLoc(name);
        _gl.Uniform3(loc, v.X, v.Y, v.Z);
    }

    public void SetVector4(string name, System.Numerics.Vector4 v)
    {
        int loc = GetLoc(name);
        _gl.Uniform4(loc, v.X, v.Y, v.Z, v.W);
    }

    public void Dispose() => _gl.DeleteProgram(Handle);
}
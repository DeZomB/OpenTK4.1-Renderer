using GL = OpenTK.Graphics.OpenGL4.GL;
using ShaderType = OpenTK.Graphics.OpenGL4.ShaderType;

namespace SimplestGame.Graphics;

public abstract class Shader : IDisposable
{
    public readonly int Program;

    public Shader(string vertexShaderSource, string fragmentShaderSource)
    {
        var vertexShader = CreateShader(vertexShaderSource, ShaderType.VertexShader);
        var fragmentshader = CreateShader(fragmentShaderSource, ShaderType.FragmentShader);

        this.Program = GL.CreateProgram();
        GL.AttachShader(this.Program, vertexShader);
        GL.AttachShader(this.Program, fragmentshader);
        GL.LinkProgram(this.Program);
        var programLog = GL.GetProgramInfoLog(this.Program);

        if (!string.IsNullOrWhiteSpace(programLog))
            Console.WriteLine(programLog);

        GL.DetachShader(this.Program, vertexShader);
        GL.DetachShader(this.Program, fragmentshader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentshader);
    }

    private static int CreateShader(string source, ShaderType shaderType)
    {
        var shader = GL.CreateShader(shaderType);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        var log = GL.GetShaderInfoLog(shader);

        if (!string.IsNullOrWhiteSpace(log))
            Console.WriteLine(log);

        return shader;
    }
    
    public virtual void Dispose()
    {
        GL.DeleteProgram(this.Program);
    }
}
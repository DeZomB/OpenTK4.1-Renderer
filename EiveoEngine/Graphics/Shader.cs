namespace EiveoEngine.Graphics;

using OpenTK.Graphics.OpenGL4;

public abstract class Shader : IDisposable
{
	protected readonly int Program;

	protected Shader(string vertexShaderSource, string fragmentShaderSource)
	{
		var vertexShader = GL.CreateShader(ShaderType.VertexShader);
		GL.ShaderSource(vertexShader, vertexShaderSource);
		GL.CompileShader(vertexShader);
		Shader.HandleErrors(GL.GetShaderInfoLog(vertexShader));

		var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
		GL.ShaderSource(fragmentShader, fragmentShaderSource);
		GL.CompileShader(fragmentShader);
		Shader.HandleErrors(GL.GetShaderInfoLog(fragmentShader));

		this.Program = GL.CreateProgram();
		GL.AttachShader(this.Program, vertexShader);
		GL.AttachShader(this.Program, fragmentShader);
		GL.LinkProgram(this.Program);
		Shader.HandleErrors(GL.GetProgramInfoLog(this.Program));

		GL.DeleteShader(vertexShader);
		GL.DeleteShader(fragmentShader);
	}

	private static void HandleErrors(string log)
	{
		if (!string.IsNullOrWhiteSpace(log))
			throw new Exception(log);
	}

	public virtual void Dispose()
	{
		GL.DeleteProgram(this.Program);
		GC.SuppressFinalize(this);
	}
}

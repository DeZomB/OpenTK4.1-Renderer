namespace EiveoEngine.Graphics;

using OpenTK.Graphics.OpenGL4;

public abstract class Shader : IDisposable
{
	protected readonly int Program;

	protected Shader(string vertexShaderSource, string fragmentShaderSource)
	{
		var vertexShader = Shader.CreateShader(vertexShaderSource, ShaderType.VertexShader);
		var fragmentshader = Shader.CreateShader(fragmentShaderSource, ShaderType.FragmentShader);

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

	public abstract void LayoutVertices();

	protected unsafe int CreateUniformBuffer<TUniforms>(string name)
		where TUniforms : unmanaged
	{
		GL.UniformBlockBinding(this.Program, GL.GetUniformBlockIndex(this.Program, name), 0);

		var uniformBuffer = GL.GenBuffer();

		var uniforms = new TUniforms();

		GL.BindBuffer(BufferTarget.UniformBuffer, uniformBuffer);
		GL.BufferData(BufferTarget.UniformBuffer, sizeof(TUniforms), ref uniforms, BufferUsageHint.StaticDraw);
		GL.BindBuffer(BufferTarget.UniformBuffer, 0);

		GL.BindBufferRange(BufferRangeTarget.UniformBuffer, 0, uniformBuffer, IntPtr.Zero, sizeof(TUniforms));

		return uniformBuffer;
	}

	public void Bind()
	{
		GL.UseProgram(this.Program);
	}


	public void Unbind()
	{
		GL.UseProgram(0);
	}

	public virtual void Dispose()
	{
		GL.DeleteProgram(this.Program);
	}
}

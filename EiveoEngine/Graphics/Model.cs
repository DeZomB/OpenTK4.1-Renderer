namespace EiveoEngine.Graphics;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Model : IDisposable
{
	private readonly int vertexBuffer;
	private readonly int indexBuffer;
	private readonly int vertexArray;

	public readonly int Indices;

	public Model(float[] vertices, uint[] indices, Shader shader)
	{
		this.Indices = indices.Length;

		this.vertexArray = GL.GenVertexArray();
		GL.BindVertexArray(this.vertexArray);

		this.vertexBuffer = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

		this.indexBuffer = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBuffer);
		GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

		shader.LayoutVertices();

		GL.BindVertexArray(0);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
	}

	public void Bind()
	{
		GL.BindVertexArray(this.vertexArray);
	}

	public void Unbind()
	{
		GL.BindVertexArray(0);
	}

	public void Dispose()
	{
		GL.DeleteVertexArray(this.vertexArray);
		GL.DeleteBuffer(this.indexBuffer);
		GL.DeleteBuffer(this.vertexBuffer);
	}
}

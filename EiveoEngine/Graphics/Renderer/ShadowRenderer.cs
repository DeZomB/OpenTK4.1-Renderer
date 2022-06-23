namespace EiveoEngine.Graphics.Renderer;

using OpenTK.Graphics.OpenGL4;
using Textures;

public class ShadowRenderer : Shader
{
	// language=glsl
	private const string VertexShader = @"
		#version 410 core

		in vec2 aPosition;
		in vec2 aUv;

		out vec2 vUv;

		void main()
		{
			vUv = aUv;
			gl_Position = vec4(aPosition, 0.0, 1.0);
		}
	";

	// language=glsl
	private const string FragmentShader = @"
		#version 410 core

		in vec2 vUv;

		out vec4 fColor;

		void main()
		{
			fColor = vec4(1.0, vUv.x, vUv.y, 1.0);
		}
	";

	private readonly int vao;
	private readonly int vbo;
	private readonly int ebo;

	public readonly FrameBufferTexture Output;
	private readonly int framebuffer;

	private int width;
	private int height;

	public ShadowRenderer()
		: base(ShadowRenderer.VertexShader, ShadowRenderer.FragmentShader)
	{
		this.vao = GL.GenVertexArray();
		GL.BindVertexArray(this.vao);

		this.vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);

		GL.BufferData(
			BufferTarget.ArrayBuffer,
			16 * sizeof(float),
			new float[] { -1, -1, 0, 0, 1, -1, 1, 0, 1, 1, 1, 1, -1, 1, 0, 1 },
			BufferUsageHint.StaticDraw
		);

		this.ebo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.ebo);
		GL.BufferData(BufferTarget.ElementArrayBuffer, 6 * sizeof(uint), new uint[] { 0, 1, 2, 2, 3, 0 }, BufferUsageHint.StaticDraw);

		var position = GL.GetAttribLocation(this.Program, "aPosition");
		GL.VertexAttribPointer(position, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0 * sizeof(float));
		GL.EnableVertexAttribArray(position);

		var uv = GL.GetAttribLocation(this.Program, "aUv");
		GL.VertexAttribPointer(uv, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
		GL.EnableVertexAttribArray(uv);

		GL.BindVertexArray(0);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

		this.framebuffer = GL.GenFramebuffer();
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.framebuffer);
		this.Output = new FrameBufferTexture(FramebufferAttachment.ColorAttachment0);
		GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}

	public void Resize(int width, int height)
	{
		if (this.width == width && this.height == height)
			return;

		this.width = width;
		this.height = height;

		this.Output.Resize(PixelInternalFormat.Rgba, width, height);
	}

	public void Draw(DeferredBuffer deferredBuffer)
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.framebuffer);

		GL.Clear(ClearBufferMask.ColorBufferBit);

		GL.UseProgram(this.Program);
		GL.BindVertexArray(this.vao);

		// TODO bind the required uniform textures!

		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

		GL.BindVertexArray(0);
		GL.UseProgram(0);

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}

	public override void Dispose()
	{
		base.Dispose();

		GL.DeleteBuffer(this.ebo);
		GL.DeleteBuffer(this.vbo);
		GL.DeleteVertexArray(this.vao);

		GC.SuppressFinalize(this);
	}
}

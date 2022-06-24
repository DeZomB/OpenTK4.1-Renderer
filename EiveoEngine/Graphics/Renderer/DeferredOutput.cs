namespace EiveoEngine.Graphics.Renderer;

using OpenTK.Graphics.OpenGL4;

public class DeferredOutput : Shader
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

		uniform sampler2D uAlbedo;
		uniform sampler2D uEmissive;
		uniform sampler2D uCube;
		uniform sampler2D uLight;

		in vec2 vUv;

		out vec4 fColor;

		void main()
		{
			vec4 albedo = texture(uAlbedo, vUv);
			vec4 emissive = texture(uEmissive, vUv);
			vec4 cube = texture(uCube, vUv);
			vec4 light = texture(uLight, vUv);
			
			fColor = mix(mix(cube, albedo * light, albedo.w), emissive, emissive.w);
		}
	";

	private readonly int vao;
	private readonly int vbo;
	private readonly int ebo;

	public DeferredOutput()
		: base(DeferredOutput.VertexShader, DeferredOutput.FragmentShader)
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
		
		GL.UseProgram(this.Program);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uAlbedo"), 0);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uEmissive"), 1);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uCube"), 2);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uLight"), 3);
		GL.UseProgram(0);
	}

	public void Draw(DeferredBuffer deferredBuffer, LightRenderer lightRenderer, ShadowRenderer shadowRenderer)
	{
		GL.UseProgram(this.Program);
		GL.BindVertexArray(this.vao);

		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Albedo.Id);

		GL.ActiveTexture(TextureUnit.Texture1);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Emissive.Id);

		GL.ActiveTexture(TextureUnit.Texture2);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Cube.Id);

		GL.ActiveTexture(TextureUnit.Texture3);
		GL.BindTexture(TextureTarget.Texture2D, lightRenderer.Output.Id);

		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

		GL.ActiveTexture(TextureUnit.Texture3);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.ActiveTexture(TextureUnit.Texture2);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.ActiveTexture(TextureUnit.Texture1);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.BindVertexArray(0);
		GL.UseProgram(0);
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

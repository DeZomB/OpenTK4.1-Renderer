namespace EiveoEngine.Graphics;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;

public class DeferredDebugger : Shader
{
	// language=glsl
	private const string VertexShader = @"
		#version 410 core

		uniform mat4 uModel;

		in vec2 aPosition;
		in vec2 aUv;

		out vec2 vUv;

		void main()
		{
			gl_Position = uModel * vec4(aPosition, 0.0, 1.0);
			vUv = aUv;
		}
	";

	// language=glsl
	private const string FragmentShader = @"
		#version 410 core

		uniform sampler2D uTexture;

		in vec2 vUv;

		out vec4 fColor;

		void main()
		{
			fColor = texture(uTexture, vUv);
		}
	";

	private readonly int model;

	private readonly int vao;
	private readonly int vbo;
	private readonly int ebo;

	public DeferredDebugger()
		: base(DeferredDebugger.VertexShader, DeferredDebugger.FragmentShader)
	{
		this.model = GL.GetUniformLocation(this.Program, "uModel");

		this.vao = GL.GenVertexArray();
		GL.BindVertexArray(this.vao);

		this.vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, 16 * sizeof(float), new float[] { 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1 }, BufferUsageHint.StaticDraw);

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
	}

	public void Draw(DeferredBuffer deferredBuffer, LightRenderer lightRenderer, ShadowRenderer shadowRenderer)
	{
		GL.UseProgram(this.Program);

		GL.ClearColor(Color.Indigo);

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		GL.ClearColor(Color.Black);

		GL.BindVertexArray(this.vao);
		GL.ActiveTexture(TextureUnit.Texture0);

		var positionMatrix = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(-1.0f, 0.0f, 0);
		GL.UniformMatrix4(this.model, false, ref positionMatrix);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Position.Id);
		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

		var normalMatrix = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(-0.5f, 0.0f, 0);
		GL.UniformMatrix4(this.model, false, ref normalMatrix);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Normal.Id);
		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

		var lightMatrix = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(0.0f, 0.0f, 0);
		GL.UniformMatrix4(this.model, false, ref lightMatrix);
		GL.BindTexture(TextureTarget.Texture2D, lightRenderer.Output.Id);
		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

		var shadowMatrix = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(0.5f, 0.0f, 0);
		GL.UniformMatrix4(this.model, false, ref shadowMatrix);
		GL.BindTexture(TextureTarget.Texture2D, shadowRenderer.Output.Id);
		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

		var albedoMatrix = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(-1.0f, -0.5f, 0);
		GL.UniformMatrix4(this.model, false, ref albedoMatrix);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Albedo.Id);
		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

		var specularMatrix = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(-0.5f, -0.5f, 0);
		GL.UniformMatrix4(this.model, false, ref specularMatrix);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Specular.Id);
		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

		var emissiveMatrix = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(0.0f, -0.5f, 0);
		GL.UniformMatrix4(this.model, false, ref emissiveMatrix);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Emissive.Id);
		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

		var cubeMatrix = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(0.5f, -0.5f, 0);
		GL.UniformMatrix4(this.model, false, ref cubeMatrix);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Cube.Id);
		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
		
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

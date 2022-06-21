namespace EiveoEngine.Graphics;

using Cameras;
using JetBrains.Annotations;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class DefaultShader : Shader
{
	// language=GLSL
	private const string VertexShader = @"
		#version 410 core

		uniform Uniforms
		{
			mat4 uView;
			mat4 uProjection;
		};

		uniform mat4 uModel;

		in vec3 aPosition;
		in vec2 aTexture;

		out vec2 vTexture;

		void main()
		{
			vTexture = aTexture;

			gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
		}
	";

	// language=GLSL
	private const string FragmentShader = @"
		#version 410 core 

		out vec4 fColor;

		uniform sampler2D uDiffuse;

		in vec2 vTexture;

		void main()
		{
			fColor = texture(uDiffuse, vTexture);
		}
	";

	
	[UsedImplicitly(ImplicitUseTargetFlags.Members)]
	private struct Uniforms
	{
		public Matrix4 View;
		public Matrix4 Projection;
	}

	private readonly int uniforms;
	private readonly int model;

	public DefaultShader()
		: base(DefaultShader.VertexShader, DefaultShader.FragmentShader)
	{
		this.uniforms = this.CreateUniformBuffer<Uniforms>("Uniforms");
		this.model = GL.GetUniformLocation(this.Program, "uModel");
	}

	public override void LayoutVertices()
	{
		var position = GL.GetAttribLocation(this.Program, "aPosition");
		GL.VertexAttribPointer(position, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
		GL.EnableVertexAttribArray(position);

		var texture = GL.GetAttribLocation(this.Program, "aTexture");
		GL.VertexAttribPointer(texture, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
		GL.EnableVertexAttribArray(texture);
	}

	public unsafe void SetCamera(Camera camera)
	{
		var uniforms = new Uniforms { View = camera.View, Projection = camera.Projection };

		GL.BindBuffer(BufferTarget.UniformBuffer, this.uniforms);
		GL.BufferData(BufferTarget.UniformBuffer, sizeof(Uniforms), ref uniforms, BufferUsageHint.StaticDraw);
		GL.BindBuffer(BufferTarget.UniformBuffer, 0);
	}

	public void SetUniforms(Matrix4 model)
	{
		GL.UniformMatrix4(this.model, false, ref model);
	}

	public override void Dispose()
	{
		GL.DeleteBuffer(this.uniforms);

		base.Dispose();
	}
}

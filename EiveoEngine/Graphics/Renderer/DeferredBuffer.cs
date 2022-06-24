namespace EiveoEngine.Graphics.Renderer;

using Cameras;
using OpenTK.Graphics.OpenGL4;
using Textures;

public class DeferredBuffer : Shader
{
	// language=glsl
	private const string VertexShader = @"
		#version 410 core

		uniform mat4 uModel;
		uniform mat4 uView;
		uniform mat4 uProjection;

		in vec3 aPosition;
		in vec3 aNormal;
		in vec3 aTangent;
		in vec3 aBiTangent;
		in vec2 aUv;

		out vec3 vPosition;
		out mat3 vTbn;
		out vec2 vUv;
		out vec3 vCubeUv;

		void main()
		{
			gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
			vPosition = (uModel * vec4(aPosition, 1.0)).xyz;

			vec3 T = normalize(vec3(uModel * vec4(aTangent, 0.0)));
			vec3 B = normalize(vec3(uModel * vec4(aBiTangent, 0.0)));
			vec3 N = normalize(vec3(uModel * vec4(aNormal, 0.0)));
			vTbn = mat3(T, B, N);

			vUv = aUv;
			vCubeUv = (inverse(mat4(mat3(uView))) * uView * uModel * vec4(aPosition, 1.0)).xyz * vec3(-1.0, -1.0, 1.0);
		}
	";

	// language=glsl
	private const string FragmentShader = @"
		#version 410 core

		uniform uMaterial
		{
			vec4 uAlbedoColor;
			vec4 uSpecularColor;
			vec4 uEmissiveColor;
			vec4 uCubeColor;

			int uAlbedoMapBound;
			int uNormalMapBound;
			int uSpecularMapBound;
			int uEmissiveMapBound;
			int uCubeMapBound;
		};

		uniform sampler2D uAlbedoMap;
		uniform sampler2D uNormalMap;
		uniform sampler2D uSpecularMap;
		uniform sampler2D uEmissiveMap;
		uniform samplerCube uCubeMap;

		in vec3 vPosition;
		in mat3 vTbn;
		in vec2 vUv;
		in vec3 vCubeUv;

		layout(location = 0) out vec4 fPosition;
		layout(location = 1) out vec4 fNormal;
		layout(location = 2) out vec4 fAlbedo;
		layout(location = 3) out vec4 fSpecular;
		layout(location = 4) out vec4 fEmissive;
		layout(location = 5) out vec4 fCube;

		void main()
		{
			fPosition = vec4(vPosition, 1.0);
			fNormal = vec4(normalize(vTbn * normalize((uNormalMapBound == 1 ? texture(uNormalMap, vUv).xyz : vec3(0.5, 0.5, 1.0)) * 2.0 - 1.0)), 1.0);
			fAlbedo = (uAlbedoMapBound == 1 ? texture(uAlbedoMap, vUv) : vec4(1.0, 1.0, 1.0, 1.0)) * uAlbedoColor;
			fSpecular = (uSpecularMapBound == 1 ? texture(uSpecularMap, vUv) : vec4(1.0, 1.0, 1.0, 1.0)) * uSpecularColor;
			fEmissive = (uEmissiveMapBound == 1 ? texture(uEmissiveMap, vUv) : vec4(1.0, 1.0, 1.0, 1.0)) * uEmissiveColor;
			fCube = (uCubeMapBound == 1 ? texture(uCubeMap, vCubeUv) : vec4(1.0, 1.0, 1.0, 1.0)) * uCubeColor;
		}
	";

	private readonly int model;
	private readonly int view;
	private readonly int projection;

	private readonly int material;

	private readonly int position;
	private readonly int normal;
	private readonly int tangent;
	private readonly int biTangent;
	private readonly int uv;

	private readonly int framebuffer;

	public readonly FrameBufferTexture Position;
	public readonly FrameBufferTexture Normal;
	public readonly FrameBufferTexture Albedo;
	public readonly FrameBufferTexture Specular;
	public readonly FrameBufferTexture Emissive;
	public readonly FrameBufferTexture Cube;

	private readonly int depth;

	private int width;
	private int height;

	public DeferredBuffer()
		: base(DeferredBuffer.VertexShader, DeferredBuffer.FragmentShader)
	{
		this.model = GL.GetUniformLocation(this.Program, "uModel");
		this.view = GL.GetUniformLocation(this.Program, "uView");
		this.projection = GL.GetUniformLocation(this.Program, "uProjection");

		this.material = GL.GetUniformBlockIndex(this.Program, "uMaterial");

		this.position = GL.GetAttribLocation(this.Program, "aPosition");
		this.normal = GL.GetAttribLocation(this.Program, "aNormal");
		this.tangent = GL.GetAttribLocation(this.Program, "aTangent");
		this.biTangent = GL.GetAttribLocation(this.Program, "aBiTangent");
		this.uv = GL.GetAttribLocation(this.Program, "aUv");

		this.framebuffer = GL.GenFramebuffer();
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.framebuffer);

		this.depth = GL.GenRenderbuffer();
		GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, this.depth);

		GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, this.depth);

		this.Position = new FrameBufferTexture(FramebufferAttachment.ColorAttachment0);
		this.Normal = new FrameBufferTexture(FramebufferAttachment.ColorAttachment1);
		this.Albedo = new FrameBufferTexture(FramebufferAttachment.ColorAttachment2);
		this.Specular = new FrameBufferTexture(FramebufferAttachment.ColorAttachment3);
		this.Emissive = new FrameBufferTexture(FramebufferAttachment.ColorAttachment4);
		this.Cube = new FrameBufferTexture(FramebufferAttachment.ColorAttachment5);

		GL.DrawBuffers(
			6,
			new[]
			{
				DrawBuffersEnum.ColorAttachment0,
				DrawBuffersEnum.ColorAttachment1,
				DrawBuffersEnum.ColorAttachment2,
				DrawBuffersEnum.ColorAttachment3,
				DrawBuffersEnum.ColorAttachment4,
				DrawBuffersEnum.ColorAttachment5
			}
		);

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

		GL.UseProgram(this.Program);

		GL.Uniform1(GL.GetUniformLocation(this.Program, "uAlbedoMap"), 0);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uNormalMap"), 1);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uSpecularMap"), 2);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uEmissiveMap"), 3);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uCubeMap"), 4);

		GL.UseProgram(0);
	}

	public void LayoutAttributes()
	{
		GL.VertexAttribPointer(this.position, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 0 * sizeof(float));
		GL.EnableVertexAttribArray(this.position);

		GL.VertexAttribPointer(this.normal, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 3 * sizeof(float));
		GL.EnableVertexAttribArray(this.normal);

		GL.VertexAttribPointer(this.tangent, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 6 * sizeof(float));
		GL.EnableVertexAttribArray(this.tangent);

		GL.VertexAttribPointer(this.biTangent, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 9 * sizeof(float));
		GL.EnableVertexAttribArray(this.biTangent);

		GL.VertexAttribPointer(this.uv, 2, VertexAttribPointerType.Float, false, 14 * sizeof(float), 12 * sizeof(float));
		GL.EnableVertexAttribArray(this.uv);
	}

	public void Resize(int width, int height)
	{
		if (this.width == width && this.height == height)
			return;

		this.width = width;
		this.height = height;

		this.Position.Resize(PixelInternalFormat.Rgba32f, width, height);
		this.Normal.Resize(PixelInternalFormat.Rgba32f, width, height);
		this.Albedo.Resize(PixelInternalFormat.Rgba, width, height);
		this.Specular.Resize(PixelInternalFormat.Rgba, width, height);
		this.Emissive.Resize(PixelInternalFormat.Rgba, width, height);
		this.Cube.Resize(PixelInternalFormat.Rgba, width, height);

		GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, this.depth);
		GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
		GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
	}

	public void Draw(Camera camera, Scene scene)
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.framebuffer);

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		GL.UseProgram(this.Program);

		var view = camera.View;
		var projection = camera.Projection;

		GL.UniformMatrix4(this.view, false, ref view);
		GL.UniformMatrix4(this.projection, false, ref projection);

		foreach (var entry in scene.ModelInstances.GroupBy(instance => instance.Material))
		{
			GL.BindBufferBase(BufferRangeTarget.UniformBuffer, this.material, entry.Key.Buffer);

			// Workaround for not having Bindless Textures...
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, entry.Key.AlbedoMap?.Id ?? 0);

			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, entry.Key.NormalMap?.Id ?? 0);

			GL.ActiveTexture(TextureUnit.Texture2);
			GL.BindTexture(TextureTarget.Texture2D, entry.Key.SpecularMap?.Id ?? 0);

			GL.ActiveTexture(TextureUnit.Texture3);
			GL.BindTexture(TextureTarget.Texture2D, entry.Key.EmissiveMap?.Id ?? 0);

			GL.ActiveTexture(TextureUnit.Texture4);
			GL.BindTexture(TextureTarget.TextureCubeMap, entry.Key.CubeMap?.Id ?? 0);

			foreach (var modelInstance in entry)
			{
				var matrix = modelInstance.Transform;
				GL.UniformMatrix4(this.model, false, ref matrix);

				modelInstance.Model.Draw();
			}
		}

		// Workaround for not having Bindless Textures...
		GL.ActiveTexture(TextureUnit.Texture4);
		GL.BindTexture(TextureTarget.TextureCubeMap, 0);

		GL.ActiveTexture(TextureUnit.Texture3);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.ActiveTexture(TextureUnit.Texture2);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.ActiveTexture(TextureUnit.Texture1);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.BindBufferBase(BufferRangeTarget.UniformBuffer, this.material, 0);

		GL.UseProgram(0);

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}

	public override void Dispose()
	{
		this.Cube.Dispose();
		this.Emissive.Dispose();
		this.Specular.Dispose();
		this.Albedo.Dispose();
		this.Normal.Dispose();
		this.Position.Dispose();

		GL.DeleteRenderbuffer(this.depth);
		GL.DeleteFramebuffer(this.framebuffer);

		base.Dispose();

		GC.SuppressFinalize(this);
	}
}

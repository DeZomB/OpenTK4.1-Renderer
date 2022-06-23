namespace EiveoEngine.Graphics.Renderer;

using Cameras;
using OpenTK.Graphics.OpenGL4;
using Textures;

public class LightRenderer : Shader
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

		struct AmbientLight {
			vec4 Color;
		};

		struct DirectionalLight {
			vec4 Color;

			vec4 Direction;
		};

		struct PointLight {
			vec4 Color;

			vec4 Position;

			float Constant;
			float Linear;
			float Quadratic;
		};

		struct SpotLight {
			vec4 Color;

			vec4 Position;
			vec4 Direction;

			float Constant;
			float Linear;
			float Quadratic;

			float CutOffInner;
			float CutOffOuter;
		};

		layout(packed) uniform uLights
		{
			AmbientLight uAmbientLights[1];
			DirectionalLight uDirectionalLights[1];
			PointLight uPointLights[1];
			SpotLight uSpotLights[1];

			vec4 vViewDirection;

			int uAmbientLightsCount;
			int uDirectionalLightsCount;
			int uPointLightsCount;
			int uSpotLightsCount;
		};

		uniform sampler2D uPositionMap;
		uniform sampler2D uNormalMap;
		uniform sampler2D uSpecularMap;

		in vec2 vUv;

		out vec4 fColor;

		void main()
		{
			vec3 position = texture(uPositionMap, vUv).xyz;
			vec3 normalMap = texture(uNormalMap, vUv).xyz;
			vec3 specularMap = texture(uSpecularMap, vUv).xyz;

			vec3 ambientLight = vec3(0.0, 0.0, 0.0);
			vec3 diffuseLight = vec3(0.0, 0.0, 0.0);
			vec3 specularLight = vec3(0.0, 0.0, 0.0);

			float shininess = 32.0;

			for (int i = 0; i < uAmbientLightsCount; i++)
			{
				vec3 lightColor = uAmbientLights[i].Color.xyz * uAmbientLights[i].Color.w;
				ambientLight += lightColor;
			}

			for (int i = 0; i < uDirectionalLightsCount; i++)
			{
				vec3 lightColor = uDirectionalLights[i].Color.xyz * uDirectionalLights[i].Color.w;
				vec3 lightDirection = normalize(-uDirectionalLights[i].Direction.xyz);
				vec3 reflectDirection = reflect(-lightDirection, normalMap);

				diffuseLight += max(dot(normalMap, lightDirection), 0.0) * lightColor;
				specularLight += specularMap.xyz * pow(max(dot(vViewDirection.xyz, reflectDirection), 0.0), shininess) * lightColor;
			}

			for (int i = 0; i < uPointLightsCount; i++)
			{
				vec3 lightColor = uPointLights[i].Color.xyz * uPointLights[i].Color.w;
				float distance = length(uPointLights[i].Position.xyz - position);
				vec3 lightDirection = normalize(uPointLights[i].Position.xyz - position);
				vec3 reflectDirection = reflect(-lightDirection, normalMap);
				float attenuation = 1.0 / (uPointLights[i].Constant + uPointLights[i].Linear * distance + uPointLights[i].Quadratic * (distance * distance));

				diffuseLight += max(dot(normalMap, lightDirection), 0.0) * lightColor * attenuation;
				specularLight += specularMap.xyz * pow(max(dot(vViewDirection.xyz, reflectDirection), 0.0), shininess) * lightColor * attenuation;
			}

			// TODO spot lights do not work yet!
			for (int i = 0; i < uSpotLightsCount; i++)
			{
				vec3 lightDirection = normalize(uSpotLights[i].Position.xyz - position);
				float theta = dot(lightDirection, normalize(-uSpotLights[i].Direction.xyz));

				if (theta > uSpotLights[i].CutOffOuter)
				{
					vec3 lightColor = uSpotLights[i].Color.xyz * uSpotLights[i].Color.w;
					float distance = length(uSpotLights[i].Position.xyz - position);
					float epsilon = uSpotLights[i].CutOffInner - uSpotLights[i].CutOffOuter;
					float intensity = clamp((theta - uSpotLights[i].CutOffOuter) / epsilon, 0.0, 1.0);
					vec3 reflectDirection = reflect(-lightDirection, normalMap);
					float attenuation = 1.0 / (uSpotLights[i].Constant + uSpotLights[i].Linear * distance + uSpotLights[i].Quadratic * (distance * distance));

					diffuseLight += max(dot(normalMap, lightDirection), 0.0) * lightColor * attenuation * intensity;
					specularLight += specularMap.xyz * pow(max(dot(vViewDirection.xyz, reflectDirection), 0.0), shininess) * lightColor * attenuation * intensity;
				}
			}

			fColor = vec4(ambientLight + diffuseLight + specularLight, 1.0);
		}
	";

	private readonly int vao;
	private readonly int vbo;
	private readonly int ebo;

	public readonly FrameBufferTexture Output;
	private readonly int framebuffer;
	private readonly int buffer;
	private readonly int lights;

	private int width;
	private int height;

	public LightRenderer()
		: base(LightRenderer.VertexShader, LightRenderer.FragmentShader)
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
		this.buffer = GL.GenBuffer();
		this.lights = GL.GetUniformBlockIndex(this.Program, "uLights");

		GL.UseProgram(this.Program);

		GL.Uniform1(GL.GetUniformLocation(this.Program, "uPositionMap"), 0);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uNormalMap"), 1);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uSpecularMap"), 2);

		GL.UseProgram(0);
	}

	public void Resize(int width, int height)
	{
		if (this.width == width && this.height == height)
			return;

		this.width = width;
		this.height = height;

		this.Output.Resize(PixelInternalFormat.Rgba, width, height);
	}

	public void Draw(DeferredBuffer deferredBuffer, Camera camera, Scene scene)
	{
		var data = new byte[(1 * 4 + 1 * 8 + 1 * 12 + 1 * 20) * sizeof(float) + 4 * sizeof(float) + 4 * sizeof(int)];
		var writer = new BinaryWriter(new MemoryStream(data));

		writer.Write(1.0f);
		writer.Write(1.0f);
		writer.Write(1.0f);
		writer.Write(0.1f);

		writer.Write(1.0f);
		writer.Write(1.0f);
		writer.Write(1.0f);
		writer.Write(0.5f);
		writer.Write(0.0f);
		writer.Write(-1.0f);
		writer.Write(0.0f);
		writer.Write(0.0f);

		writer.Write(1.0f);
		writer.Write(0.0f);
		writer.Write(0.0f);
		writer.Write(1.0f);
		writer.Write(10.0f);
		writer.Write(2.0f);
		writer.Write(10.0f);
		writer.Write(0.0f);
		writer.Write(1.0f);
		writer.Write(0.045f);
		writer.Write(0.0075f);
		writer.Write(0.0f);

		writer.Write(0.0f);
		writer.Write(1.0f);
		writer.Write(0.0f);
		writer.Write(1.0f);
		writer.Write(10.0f);
		writer.Write(-2.0f);
		writer.Write(10.0f);
		writer.Write(0.0f);
		writer.Write(0.0f);
		writer.Write(1.0f);
		writer.Write(0.0f);
		writer.Write(0.0f);
		writer.Write(1);
		writer.Write(0.045f);
		writer.Write(0.0075f);
		writer.Write(12.5f);
		writer.Write(17.5f);
		writer.Write(0.0f);
		writer.Write(0.0f);
		writer.Write(0.0f);

		writer.Write(camera.Forward.X);
		writer.Write(camera.Forward.Y);
		writer.Write(camera.Forward.Z);
		writer.Write(0.0f);

		writer.Write(1);
		writer.Write(1);
		writer.Write(1);
		writer.Write(1);

		GL.BindBuffer(BufferTarget.UniformBuffer, this.buffer);
		GL.BufferData(BufferTarget.UniformBuffer, data.Length, data, BufferUsageHint.StaticDraw);
		GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.framebuffer);

		GL.Clear(ClearBufferMask.ColorBufferBit);

		GL.UseProgram(this.Program);
		GL.BindBufferBase(BufferRangeTarget.UniformBuffer, this.lights, this.buffer);
		GL.BindVertexArray(this.vao);

		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Position.Id);

		GL.ActiveTexture(TextureUnit.Texture1);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Normal.Id);

		GL.ActiveTexture(TextureUnit.Texture2);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Specular.Id);

		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

		GL.ActiveTexture(TextureUnit.Texture2);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.ActiveTexture(TextureUnit.Texture1);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.BindVertexArray(0);
		GL.BindBufferBase(BufferRangeTarget.UniformBuffer, this.lights, 0);
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

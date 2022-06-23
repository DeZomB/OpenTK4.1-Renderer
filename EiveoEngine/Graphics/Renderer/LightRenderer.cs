namespace EiveoEngine.Graphics.Renderer;

using Cameras;
using Lights;
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

		struct Light {
			vec4 Color;

			vec4 Position;
			vec4 Direction;

			float Constant;
			float Linear;
			float Quadratic;

			float CutOffInner;
			float CutOffOuter;

			int Type;
		};

		uniform uUniforms
		{
			Light uLights[512];
			vec4 vViewDirection;
		};

		uniform sampler2D uLastMap;
		uniform sampler2D uPositionMap;
		uniform sampler2D uNormalMap;
		uniform sampler2D uSpecularMap;

		in vec2 vUv;

		out vec4 fColor;

		void main()
		{
			vec3 lastMap = texture(uLastMap, vUv).xyz;
			vec3 position = texture(uPositionMap, vUv).xyz;
			vec3 normalMap = texture(uNormalMap, vUv).xyz;
			vec3 specularMap = texture(uSpecularMap, vUv).xyz;

			vec3 ambientLight = vec3(0.0, 0.0, 0.0);
			vec3 diffuseLight = vec3(0.0, 0.0, 0.0);
			vec3 specularLight = vec3(0.0, 0.0, 0.0);

			float shininess = 32.0;

			for (int i = 0; i < 512; i++)
			{
				if (uLights[i].Type == 0)
				{
					break;
				}
				else if (uLights[i].Type == 1)
				{
					vec3 lightColor = uLights[i].Color.xyz * uLights[i].Color.w;
					ambientLight += lightColor;
				}
				else if (uLights[i].Type == 2)
				{
					vec3 lightColor = uLights[i].Color.xyz * uLights[i].Color.w;
					vec3 lightDirection = -uLights[i].Direction.xyz;
					vec3 reflectDirection = reflect(-lightDirection, normalMap);

					diffuseLight += max(dot(normalMap, lightDirection), 0.0) * lightColor;
					specularLight += specularMap.xyz * pow(max(dot(vViewDirection.xyz, reflectDirection), 0.0), shininess) * lightColor;
				}
				else if (uLights[i].Type == 3)
				{
					vec3 lightColor = uLights[i].Color.xyz * uLights[i].Color.w;
					float distance = length(uLights[i].Position.xyz - position);
					vec3 lightDirection = normalize(uLights[i].Position.xyz - position);
					vec3 reflectDirection = reflect(-lightDirection, normalMap);
					float attenuation = 1.0 / (uLights[i].Constant + uLights[i].Linear * distance + uLights[i].Quadratic * (distance * distance));

					diffuseLight += max(dot(normalMap, lightDirection), 0.0) * lightColor * attenuation;
					specularLight += specularMap.xyz * pow(max(dot(vViewDirection.xyz, reflectDirection), 0.0), shininess) * lightColor * attenuation;
				}
				else if (uLights[i].Type == 4)
				{
					vec3 lightDirection = normalize(uLights[i].Position.xyz - position);
					float theta = dot(lightDirection, -uLights[i].Direction.xyz);
					vec3 lightColor = uLights[i].Color.xyz * uLights[i].Color.w;
					float distance = length(uLights[i].Position.xyz - position);
					float epsilon = uLights[i].CutOffInner - uLights[i].CutOffOuter;
					float intensity = clamp((theta - uLights[i].CutOffOuter) / epsilon, 0.0, 1.0);
					vec3 reflectDirection = reflect(-lightDirection, normalMap);
					float attenuation = 1.0 / (uLights[i].Constant + uLights[i].Linear * distance + uLights[i].Quadratic * (distance * distance));

					diffuseLight += max(dot(normalMap, lightDirection), 0.0) * lightColor * attenuation * intensity;
					specularLight += specularMap.xyz * pow(max(dot(vViewDirection.xyz, reflectDirection), 0.0), shininess) * lightColor * attenuation * intensity;
				}
			}

			fColor = vec4(lastMap + ambientLight + diffuseLight + specularLight, 1.0);
		}
	";

	private readonly int vao;
	private readonly int vbo;
	private readonly int ebo;

	public readonly FrameBufferTexture Output;
	private readonly int framebuffer;
	private readonly int buffer;
	private readonly int uniforms;

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
		this.uniforms = GL.GetUniformBlockIndex(this.Program, "uUniforms");

		GL.UseProgram(this.Program);

		GL.Uniform1(GL.GetUniformLocation(this.Program, "uLastMap"), 0);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uPositionMap"), 1);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uNormalMap"), 2);
		GL.Uniform1(GL.GetUniformLocation(this.Program, "uSpecularMap"), 3);

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
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.framebuffer);

		GL.Clear(ClearBufferMask.ColorBufferBit);

		GL.UseProgram(this.Program);
		GL.BindBufferBase(BufferRangeTarget.UniformBuffer, this.uniforms, this.buffer);
		GL.BindVertexArray(this.vao);

		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, this.Output.Id);

		GL.ActiveTexture(TextureUnit.Texture1);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Position.Id);

		GL.ActiveTexture(TextureUnit.Texture2);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Normal.Id);

		GL.ActiveTexture(TextureUnit.Texture3);
		GL.BindTexture(TextureTarget.Texture2D, deferredBuffer.Specular.Id);

		var lightBatches = scene.Lights.Select((value, index) => new { Value = value, Index = index })
			.GroupBy(i => i.Index / 512, v => v.Value)
			.Select(e => e.ToArray())
			.ToArray();

		foreach (var lights in lightBatches)
		{
			var data = new byte[(512 * 20 + 4) * sizeof(float)];
			var writer = new BinaryWriter(new MemoryStream(data));

			foreach (var light in lights)
			{
				writer.Write(light.Color.R / 255f);
				writer.Write(light.Color.G / 255f);
				writer.Write(light.Color.B / 255f);
				writer.Write(light.Color.A / 255f);

				switch (light)
				{
					case AmbientLight:
						writer.BaseStream.Position += 13 * sizeof(float);
						writer.Write(1);

						break;

					case DirectionalLight directionalLight:
						writer.BaseStream.Position += 4 * sizeof(float);
						writer.Write(directionalLight.Direction.X);
						writer.Write(directionalLight.Direction.Y);
						writer.Write(directionalLight.Direction.Z);
						writer.Write(1f);
						writer.BaseStream.Position += 5 * sizeof(float);
						writer.Write(2);

						break;

					case PointLight pointLight:
						writer.Write(pointLight.Position.X);
						writer.Write(pointLight.Position.Y);
						writer.Write(pointLight.Position.Z);
						writer.Write(1f);
						writer.BaseStream.Position += 4 * sizeof(float);
						writer.Write(pointLight.Constant);
						writer.Write(pointLight.Linear);
						writer.Write(pointLight.Quadratic);
						writer.BaseStream.Position += 2 * sizeof(float);
						writer.Write(3);

						break;

					case SpotLight spotLight:
						writer.Write(spotLight.Position.X);
						writer.Write(spotLight.Position.Y);
						writer.Write(spotLight.Position.Z);
						writer.Write(1f);
						writer.Write(spotLight.Direction.X);
						writer.Write(spotLight.Direction.Y);
						writer.Write(spotLight.Direction.Z);
						writer.Write(1f);
						writer.Write(spotLight.Constant);
						writer.Write(spotLight.Linear);
						writer.Write(spotLight.Quadratic);
						writer.Write(spotLight.CutOffInner);
						writer.Write(spotLight.CutOffOuter);
						writer.Write(4);

						break;
				}

				writer.BaseStream.Position += 2 * sizeof(float);
			}

			writer.BaseStream.Position = 512 * 20 * sizeof(float);
			writer.Write(camera.Forward.X);
			writer.Write(camera.Forward.Y);
			writer.Write(camera.Forward.Z);
			writer.Write(0.0f);

			GL.BindBuffer(BufferTarget.UniformBuffer, this.buffer);
			GL.BufferData(BufferTarget.UniformBuffer, data.Length, data, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);

			GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
		}

		GL.ActiveTexture(TextureUnit.Texture3);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.ActiveTexture(TextureUnit.Texture2);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.ActiveTexture(TextureUnit.Texture1);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.BindVertexArray(0);
		GL.BindBufferBase(BufferRangeTarget.UniformBuffer, this.uniforms, 0);
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

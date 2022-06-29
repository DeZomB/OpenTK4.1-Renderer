namespace EiveoEngine.Graphics.Renderer;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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
	
	// --------------------------------------------------------------		
	/*
	 * Render the Scene from the lights view
	 *	Spotlight with perspective view
	 *	Directional light with orthographic view
	 *
	 * Perform depth test
	 * Store the depth values in a depth map/ shadow map
	 *
	 *	1. Render the depth map
	 *		Create a framebuffer object
	 *		Create a 2D texture as a buffer (1k resolution)
	 *		No need for a color buffer
	 *		Render the depthmap
	 *		Store the result in a texture
	 *  1.1 Render the scene as normal and calculate with the depth map whether a fragment is in the shadow
	 *		Light space transform
	 *		Because we're modelling a directional light source, all its light rays are parallel. For this reason,
	 *		we're going to use an orthographic projection matrix for the light source where there is no perspective deform.
	 *		Setup the correct near_plane and far_plane. When objects or fragments are not in the depth map they will not produce shadows.
	 *	2.	Render depth map.
	 *	3.	Render Shadows
	 *		
	 */
	
	private int depthmapFrameBufferObject;
	private int depthmap;
	private readonly int shadow_width = 1024;
	private readonly int shadow_height = 1024;

	private Matrix4 lightprojection;
	private readonly float near_plane = 1f;
	private readonly float far_plane = 0.75f;
	private Matrix4 lightview;
	public void RenderLight(Vector3 lightposition, Vector3 lightstarget)
	{
		lightstarget = Vector3.Zero;
		
		this.depthmapFrameBufferObject = GL.GenBuffer();
		this.depthmap = GL.GenTexture();
		
		GL.BindTexture(TextureTarget.Texture2D, this.depthmap);
		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, this.shadow_width, this.shadow_height, 0,PixelFormat.Rgba, PixelType.Float , IntPtr.Zero);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);

		GL.BindBuffer(BufferTarget.ShaderStorageBuffer, this.depthmapFrameBufferObject);
		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, this.depthmap, 0);
		GL.DrawBuffer(DrawBufferMode.None);
		GL.ReadBuffer(ReadBufferMode.None);
		GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

		// 1. render depth of scene to texture (from light's perspective)
		// --------------------------------------------------------------
		this.lightprojection = Matrix4.CreateOrthographicOffCenter(-10f, 10f,-10f,10f, this.near_plane, this.framebuffer);

		this.lightview = Matrix4.LookAt(lightposition, lightstarget, Vector3.UnitY);

		var lightSpaceMatrix = this.lightprojection * this.lightview;
		
		GL.Viewport(0,0, this.shadow_width, this.shadow_height);
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.depthmapFrameBufferObject);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		
		// 2. render scene as normal using the generated depth/shadow map  
		// --------------------------------------------------------------
		glViewport(0, 0, SCR_WIDTH, SCR_HEIGHT);
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
		shader.use();
		glm::mat4 projection = glm::perspective(glm::radians(camera.Zoom), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 100.0f);
		glm::mat4 view = camera.GetViewMatrix();
		shader.setMat4("projection", projection);
		shader.setMat4("view", view);
		// set light uniforms
		shader.setVec3("viewPos", camera.Position);
		shader.setVec3("lightPos", lightPos);
		shader.setMat4("lightSpaceMatrix", lightSpaceMatrix);
		glActiveTexture(GL_TEXTURE0);
		glBindTexture(GL_TEXTURE_2D, woodTexture);
		glActiveTexture(GL_TEXTURE1);
		glBindTexture(GL_TEXTURE_2D, depthMap);
		renderScene(shader);
		
		// render Depth map to quad for visual debugging
		// ---------------------------------------------
		debugDepthQuad.use();
		debugDepthQuad.setFloat("near_plane", near_plane);
		debugDepthQuad.setFloat("far_plane", far_plane);
		glActiveTexture(GL_TEXTURE0);
		glBindTexture(GL_TEXTURE_2D, depthMap);
		
	}
	// --------------------------------------------------------------
	
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

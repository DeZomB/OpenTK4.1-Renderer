using OpenTK.Graphics.OpenGL4;

namespace EiveoEngine.Graphics;

public class GeometryBuffer
{

	private int gBuffer;
	private int gPosition;
	private int gNormal;
	private int gAlbedoSpec;
	
	public void DebugBufferOutput(int width, int height)
	{
		this.gBuffer = GL.GenBuffer();
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.gBuffer);

		GL.GenTextures(1, out this.gPosition);
		GL.BindTexture(TextureTarget.Texture2D, this.gPosition);

		GL.TexImage2D(
			TextureTarget.Texture2D,
			0,
			PixelInternalFormat.Rgba16f,
			width,
			height,
			0,
			PixelFormat.Rgba,
			PixelType.Float,
			IntPtr.Zero
		);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

		GL.FramebufferTexture2D(
			FramebufferTarget.Framebuffer,
			FramebufferAttachment.ColorAttachment0,
			TextureTarget.Texture2D,
			this.gPosition,
			0
		);

		GL.GenTextures(1, out this.gNormal);
		GL.BindTexture(TextureTarget.Texture2D, this.gNormal);

		GL.TexImage2D(
			TextureTarget.Texture2D,
			0,
			PixelInternalFormat.Rgba16f,
			width,
			height,
			0,
			PixelFormat.Rgba,
			PixelType.Float,
			IntPtr.Zero //When calling the Windows API from managed code, you can pass IntPtr.Zero instead of null if an argument is expected to be either a pointer or a null. 
		);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

		GL.FramebufferTexture2D(
			FramebufferTarget.Framebuffer,
			FramebufferAttachment.ColorAttachment1,
			TextureTarget.Texture2D,
			this.gNormal,
			0
		);

		GL.GenTextures(1, out this.gAlbedoSpec);
		GL.BindTexture(TextureTarget.Texture2D, this.gAlbedoSpec);

		GL.TexImage2D(
			TextureTarget.Texture2D,
			0,
			PixelInternalFormat.Rgba,
			width,
			height,
			0,
			PixelFormat.Rgba,
			PixelType.UnsignedByte,
			IntPtr.Zero
		);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

		GL.FramebufferTexture2D(
			FramebufferTarget.Framebuffer,
			FramebufferAttachment.ColorAttachment2,
			TextureTarget.Texture2D,
			this.gAlbedoSpec,
			0
		);

		var attachment = new[]
		{
			DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2
		};

		GL.DrawBuffers(3, attachment);

		// then also add render buffer object as depth buffer and check for completeness.
	}
}

namespace EiveoEngine.Graphics.Textures;

using OpenTK.Graphics.OpenGL4;

public class FrameBufferTexture : Texture
{
	public FrameBufferTexture(FramebufferAttachment framebufferAttachment)
		: base(TextureTarget.Texture2D)
	{
		GL.BindTexture(this.TextureTarget, this.Id);

		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, framebufferAttachment, TextureTarget.Texture2D, this.Id, 0);

		this.SetParameters();

		GL.BindTexture(TextureTarget.Texture2D, 0);
	}

	protected override void SetParameters()
	{
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
	}

	public void Resize(PixelInternalFormat internalFormat, int width, int height)
	{
		GL.BindTexture(TextureTarget.Texture2D, this.Id);
		GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
		GL.BindTexture(TextureTarget.Texture2D, 0);
	}
}

namespace EiveoEngine.Graphics;

using OpenTK.Graphics.OpenGL4;

public class Texture : IDisposable
{
	private readonly int handle;

	public readonly int Width;
	public readonly int Height;

	public Texture(byte[] pixels, int width, int height)
	{
		this.Width = width;
		this.Height = height;

		this.handle = GL.GenTexture();

		GL.BindTexture(TextureTarget.Texture2D, this.handle);

		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);

		GL.BindTexture(TextureTarget.Texture2D, 0);
	}

	public void Bind()
	{
		GL.BindTexture(TextureTarget.Texture2D, this.handle);
	}

	public void Unbind()
	{
		GL.BindTexture(TextureTarget.Texture2D, 0);
	}

	public void Dispose()
	{
		GL.DeleteTexture(this.handle);
	}
}

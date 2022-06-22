namespace EiveoEngine.Graphics.Textures;

using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

public abstract class Texture : IDisposable
{
	protected readonly TextureTarget TextureTarget;
	public readonly int Id;

	protected Texture(TextureTarget textureTarget)
	{
		this.TextureTarget = textureTarget;

		this.Id = GL.GenTexture();
	}

	protected unsafe void LoadImage(string path, TextureTarget subTarget)
	{
		var image = Image.Load<Rgba32>(File.ReadAllBytes(path));
		var pixels = new byte[image.Width * image.Width * sizeof(Rgba32)];

		image.Mutate(context => context.Flip(FlipMode.Vertical));
		image.CopyPixelDataTo(pixels);

		GL.BindTexture(this.TextureTarget, this.Id);

		GL.TexImage2D(subTarget, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

		this.SetParameters();

		GL.BindTexture(TextureTarget.Texture2D, 0);
	}

	protected abstract void SetParameters();

	public void Dispose()
	{
		GL.DeleteTexture(this.Id);
		GC.SuppressFinalize(this);
	}
}

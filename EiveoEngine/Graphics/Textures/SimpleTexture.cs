namespace EiveoEngine.Graphics.Textures;

using OpenTK.Graphics.OpenGL4;

public class SimpleTexture : Texture
{
	public SimpleTexture(string path)
		: base(TextureTarget.Texture2D)
	{
		this.LoadImage($"{path}.png", TextureTarget.Texture2D);
	}

	protected override void SetParameters()
	{
		GL.TexParameter(this.TextureTarget, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
		GL.TexParameter(this.TextureTarget, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);

		GL.TexParameter(this.TextureTarget, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
		GL.TexParameter(this.TextureTarget, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
	}
}

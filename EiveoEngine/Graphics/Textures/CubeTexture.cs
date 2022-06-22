namespace EiveoEngine.Graphics.Textures;

using OpenTK.Graphics.OpenGL4;

public class CubeTexture : Texture
{
	private static readonly Dictionary<TextureTarget, string> CubeMap = new()
	{
		{ TextureTarget.TextureCubeMapPositiveX, "right" },
		{ TextureTarget.TextureCubeMapNegativeX, "left" },
		{ TextureTarget.TextureCubeMapPositiveY, "bottom" }, // TODO should be top ?!
		{ TextureTarget.TextureCubeMapNegativeY, "top" }, // TODO should be bottom ?!
		{ TextureTarget.TextureCubeMapPositiveZ, "front" },
		{ TextureTarget.TextureCubeMapNegativeZ, "back" }
	};

	public CubeTexture(string path)
		: base(TextureTarget.TextureCubeMap)
	{
		foreach (var (subTextureTarget, suffix) in CubeTexture.CubeMap)
			this.LoadImage($"{path}_{suffix}.png", subTextureTarget);
	}

	protected override void SetParameters()
	{
		GL.TexParameter(this.TextureTarget, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
		GL.TexParameter(this.TextureTarget, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
		GL.TexParameter(this.TextureTarget, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToEdge);

		GL.TexParameter(this.TextureTarget, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
		GL.TexParameter(this.TextureTarget, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
	}
}

using OpenTK.Graphics.OpenGL4;

namespace SimplestGame.Graphics;

public class Texture : IDisposable
{
    public readonly int Handle;
    public readonly int Width;
    public readonly int Height;

    public Texture(byte[] pixels, int width, int height)
    {
        this.Width = width;
        this.Height = height;

        this.Handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, this.Handle);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, pixels);
        
        //Target Type - Filtertype - filterspecs
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }


    public void Dispose()
    {
        GL.DeleteTexture(this.Handle);
    }
}
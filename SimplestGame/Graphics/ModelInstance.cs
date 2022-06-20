using OpenTK.Graphics.OpenGL4;

namespace SimplestGame.Graphics;

public class ModelInstance
{
    private readonly Model model;
    private readonly Texture texture;

    public ModelInstance(Model model, Texture texture)
    {
        this.model = model;
        this.texture = texture;
    }

    public void Render(TextureUnit unit)
    {
        GL.ActiveTexture(unit); //(?)
        GL.BindTexture(TextureTarget.Texture2D, this.texture.Handle);
        this.model.Render();
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }
}
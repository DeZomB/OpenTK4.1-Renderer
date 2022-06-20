using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SimplestGame.Graphics;

public class ModelInstance
{
    private readonly Model model;
    private readonly Texture texture;

    public Vector3 Position;
    public Vector3 Scale = Vector3.One;
    public Quaternion Rotation = Quaternion.Identity;

    public ModelInstance(Model model, Texture texture)
    {
        this.model = model;
        this.texture = texture;
    }

    public void Render()
    {
        GL.BindTexture(TextureTarget.Texture2D, this.texture.Handle);
        this.model.Render(Matrix4.CreateTranslation(this.Position) * Matrix4.CreateFromQuaternion(this.Rotation) * Matrix4.CreateScale(this.Scale));
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }
}
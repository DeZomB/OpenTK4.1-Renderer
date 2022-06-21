namespace EiveoEngine.Graphics;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class ModelInstance
{
	private readonly Model model;
	private readonly Texture texture;
	private readonly DefaultShader shader;

	public Vector3 Position;
	public Vector3 Scale = Vector3.One;
	public Quaternion Rotation = Quaternion.Identity;

	public ModelInstance(Model model, Texture texture, DefaultShader shader)
	{
		this.model = model;
		this.texture = texture;
		this.shader = shader;
	}

	public void Render()
	{
		this.shader.Bind();
		this.model.Bind();
		this.texture.Bind();

		this.shader.SetUniforms(Matrix4.CreateTranslation(this.Position) * Matrix4.CreateFromQuaternion(this.Rotation) * Matrix4.CreateScale(this.Scale));

		GL.DrawElements(PrimitiveType.Triangles, this.model.Indices, DrawElementsType.UnsignedInt, 0);

		this.texture.Unbind();
		this.model.Unbind();
		this.shader.Unbind();
	}
}

namespace EiveoEngine.Graphics;

using OpenTK.Mathematics;

public class ModelInstance
{
	public readonly Model Model;
	public readonly Material Material;

	private Vector3 position = Vector3.Zero;

	public Vector3 Position
	{
		get => this.position;
		set
		{
			this.position = value;
			this.dirty = true;
		}
	}

	private Vector3 rotation = Vector3.Zero;

	public Vector3 Rotation
	{
		get => this.position;
		set
		{
			this.rotation = value;
			this.dirty = true;
		}
	}

	private Vector3 scale = Vector3.One;

	public Vector3 Scale
	{
		get => this.position;
		set
		{
			this.scale = value;
			this.dirty = true;
		}
	}

	private bool dirty = true;
	private Matrix4 transform;

	public Matrix4 Transform
	{
		get
		{
			if (!this.dirty)
				return this.transform;

			this.transform = Matrix4.CreateScale(this.scale) *
				Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(this.rotation.X, this.rotation.Y, this.rotation.Z)) *
				Matrix4.CreateTranslation(this.position);

			this.dirty = false;

			return this.transform;
		}
	}

	public ModelInstance(Model model, Material material)
	{
		this.Model = model;
		this.Material = material;
	}
}

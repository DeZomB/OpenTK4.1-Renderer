namespace EiveoEngine.Graphics.Cameras;

using Extensions;
using OpenTK.Mathematics;

public abstract class Camera
{
	public float Pitch;
	public float Yaw = -90;
	public float Roll;

	public Vector3 Postion;

	public Vector3 Forward { get; private set; }
	public Vector3 Right { get; private set; }

	public Matrix4 View { get; private set; }
	public Matrix4 Projection { get; private set; }

	public Vector2 Size { get; private set; }

	public void Update(Vector2 size)
	{
		this.Size = size;

		this.Forward = new Vector3
		{
			X = (float) (Math.Cos(this.Yaw.ToRadians()) * Math.Cos(this.Pitch.ToRadians())),
			Y = (float) Math.Sin(this.Pitch.ToRadians()),
			Z = (float) (Math.Sin(this.Yaw.ToRadians()) * Math.Cos(this.Pitch.ToRadians()))
		}.Normalized();

		this.Right = Vector3.Cross(this.Forward, Vector3.UnitY).Normalized();

		this.View = Matrix4.LookAt(this.Postion, this.Postion + this.Forward, Vector3.UnitY);
		this.Projection = this.CreateProjection(size);
	}

	protected abstract Matrix4 CreateProjection(Vector2 size);
}

namespace EiveoEngine.Cameras;

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

	public void Update(Vector2 size)
	{
		this.Forward = new Vector3
		{
			X = (float) (Math.Cos(MathHelper.DegreesToRadians(this.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(this.Pitch))),
			Y = (float) Math.Sin(MathHelper.DegreesToRadians(this.Pitch)),
			Z = (float) (Math.Sin(MathHelper.DegreesToRadians(this.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(this.Pitch)))
		}.Normalized();

		this.Right = Vector3.Cross(this.Forward, Vector3.UnitY).Normalized();

		this.View = this.CreateView();
		this.Projection = this.CreateProjection(size);
	}

	protected abstract Matrix4 CreateView();
	protected abstract Matrix4 CreateProjection(Vector2 size);
}

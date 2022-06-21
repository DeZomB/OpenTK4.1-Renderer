namespace EiveoEngine.Graphics;

using OpenTK.Mathematics;

public class Camera
{
	public float Pitch;
	public float Yaw = -90;

	public float Fov = 90;

	public Vector3 Postion;

	public Vector3 Forward { get; private set; }
	public Vector3 Right { get; private set; }

	public Matrix4 View = Matrix4.Identity;
	public Matrix4 Projection = Matrix4.Identity;

	public void Update(Vector2 size)
	{
		this.Forward = new Vector3
		{
			X = (float) (Math.Cos(MathHelper.DegreesToRadians(this.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(this.Pitch))),
			Y = (float) Math.Sin(MathHelper.DegreesToRadians(this.Pitch)),
			Z = (float) (Math.Sin(MathHelper.DegreesToRadians(this.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(this.Pitch)))
		}.Normalized();

		this.Right = Vector3.Cross(this.Forward, Vector3.UnitY).Normalized();

		this.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(this.Fov), size.X / size.Y, 1, short.MaxValue);
		this.View = Matrix4.LookAt(this.Postion, this.Postion + this.Forward, Vector3.UnitY);
	}
}

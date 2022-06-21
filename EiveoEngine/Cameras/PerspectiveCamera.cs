namespace EiveoEngine.Cameras;

using OpenTK.Mathematics;

public class PerspectiveCamera : Camera
{
	public float Fov = 90;

	protected override Matrix4 CreateView()
	{
		return Matrix4.LookAt(this.Postion, this.Postion + this.Forward, Vector3.UnitY);
	}

	protected override Matrix4 CreateProjection(Vector2 size)
	{
		return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(this.Fov), size.X / size.Y, 1, short.MaxValue);
	}
}

namespace EiveoEngine.Graphics.Cameras;

using Extensions;
using OpenTK.Mathematics;

public class PerspectiveCamera : Camera
{
	public float Fov = 90;

	protected override Matrix4 CreateProjection(Vector2 size)
	{
		return Matrix4.CreatePerspectiveFieldOfView(this.Fov.ToRadians(), size.X / size.Y, .1f, short.MaxValue);
	}
}

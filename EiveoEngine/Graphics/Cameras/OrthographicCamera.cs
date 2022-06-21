namespace EiveoEngine.Graphics.Cameras;

using OpenTK.Mathematics;

public class OrthographicCamera : Camera
{
	public float Zoom = 1;

	protected override Matrix4 CreateProjection(Vector2 size)
	{
		return Matrix4.CreateOrthographicOffCenter(
			size.X / -2 / this.Zoom,
			size.X / 2 / this.Zoom,
			size.Y / -2 / this.Zoom,
			size.Y / 2 / this.Zoom,
			1,
			short.MaxValue
		);
	}
}

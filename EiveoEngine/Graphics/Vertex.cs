namespace EiveoEngine.Graphics;

using OpenTK.Mathematics;

public struct Vertex
{
	public readonly Vector3 Position;
	public readonly Vector3 Normal;
	public readonly Vector2 Uv;

	public Vertex(Vector3 position, Vector3 normal, Vector2 uv)
	{
		this.Position = position;
		this.Normal = normal;
		this.Uv = uv;
	}
}

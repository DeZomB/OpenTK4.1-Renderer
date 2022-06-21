namespace EiveoEngine.Graphics;

using JetBrains.Annotations;
using OpenTK.Mathematics;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public readonly struct Vertex
{
	public readonly Vector3 Position;
	public readonly Vector2 Texture;

	public Vertex(Vector3 position, Vector2 texture)
	{
		this.Position = position;
		this.Texture = texture;
	}
}

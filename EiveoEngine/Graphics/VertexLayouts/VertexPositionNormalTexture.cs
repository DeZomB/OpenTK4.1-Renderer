namespace EiveoEngine.Graphics.VertexLayouts;

using JetBrains.Annotations;
using OpenTK.Mathematics;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public readonly struct VertexPositionNormalTexture
{
	public readonly Vector3 Position;
	public readonly Vector3 Normal;
	public readonly Vector2 Texture;

	public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texture)
	{
		this.Position = position;
		this.Normal = normal;
		this.Texture = texture;
	}
}

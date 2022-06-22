namespace EiveoEngine.Graphics;

public class Index
{
	public readonly uint Vertex1;
	public readonly uint Vertex2;
	public readonly uint Vertex3;

	public Index(uint vertex1, uint vertex2, uint vertex3)
	{
		this.Vertex1 = vertex1;
		this.Vertex2 = vertex2;
		this.Vertex3 = vertex3;
	}
}

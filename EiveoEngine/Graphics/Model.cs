namespace EiveoEngine.Graphics;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Model : IDisposable
{
	private readonly int elements;

	private readonly int vao;
	private readonly int vbo;
	private readonly int ebo;

	public Model(DeferredBuffer deferredBuffer, IReadOnlyList<Vertex> vertices, IReadOnlyCollection<Index> indices)
	{
		this.elements = indices.Count * 3;

		this.vao = GL.GenVertexArray();
		GL.BindVertexArray(this.vao);

		this.vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
		var vertexData = Model.BuildVertexData(vertices, indices);
		GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

		this.ebo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.ebo);
		var indexData = Enumerable.Range(0, this.elements).Select(i => (uint) i).ToArray();
		GL.BufferData(BufferTarget.ElementArrayBuffer, indexData.Length * sizeof(uint), indexData, BufferUsageHint.StaticDraw);

		deferredBuffer.LayoutAttributes();

		GL.BindVertexArray(0);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
	}

	private static float[] BuildVertexData(IReadOnlyList<Vertex> vertices, IEnumerable<Index> indices)
	{
		var data = new List<float>();

		foreach (var index in indices)
		{
			var vertex1 = vertices[(int) index.Vertex1];
			var vertex2 = vertices[(int) index.Vertex2];
			var vertex3 = vertices[(int) index.Vertex3];

			var edge1 = vertex2.Position - vertex1.Position;
			var edge2 = vertex3.Position - vertex1.Position;
			var deltaUv1 = vertex2.Uv - vertex1.Uv;
			var deltaUv2 = vertex3.Uv - vertex1.Uv;

			var f = 1.0f / (deltaUv1.X * deltaUv2.Y - deltaUv2.X * deltaUv1.Y);

			var tangent = new Vector3(
				f * (deltaUv2.Y * edge1.X - deltaUv1.Y * edge2.X),
				f * (deltaUv2.Y * edge1.Y - deltaUv1.Y * edge2.Y),
				f * (deltaUv2.Y * edge1.Z - deltaUv1.Y * edge2.Z)
			);

			var bitangent = new Vector3(
				f * (-deltaUv2.X * edge1.X + deltaUv1.X * edge2.X),
				f * (-deltaUv2.X * edge1.Y + deltaUv1.X * edge2.Y),
				f * (-deltaUv2.X * edge1.Z + deltaUv1.X * edge2.Z)
			);

			Model.BuildData(data, vertex1, tangent, bitangent);
			Model.BuildData(data, vertex2, tangent, bitangent);
			Model.BuildData(data, vertex3, tangent, bitangent);
		}

		return data.ToArray();
	}

	private static void BuildData(ICollection<float> data, Vertex vertex, Vector3 tangent, Vector3 bitangent)
	{
		data.Add(vertex.Position.X);
		data.Add(vertex.Position.Y);
		data.Add(vertex.Position.Z);
		data.Add(vertex.Normal.X);
		data.Add(vertex.Normal.Y);
		data.Add(vertex.Normal.Z);
		data.Add(tangent.X);
		data.Add(tangent.Y);
		data.Add(tangent.Z);
		data.Add(bitangent.X);
		data.Add(bitangent.Y);
		data.Add(bitangent.Z);
		data.Add(vertex.Uv.X);
		data.Add(vertex.Uv.Y);
	}

	public void Draw()
	{
		GL.BindVertexArray(this.vao);

		GL.DrawElements(PrimitiveType.Triangles, this.elements, DrawElementsType.UnsignedInt, 0);

		GL.BindVertexArray(0);
	}

	public void Dispose()
	{
		GL.DeleteBuffer(this.ebo);
		GL.DeleteBuffer(this.vbo);
		GL.DeleteVertexArray(this.vao);

		GC.SuppressFinalize(this);
	}
}

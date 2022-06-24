namespace EiveoEngine.Graphics;

using JetBrains.Annotations;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Renderer;

public class Model : IDisposable
{
	[UsedImplicitly(ImplicitUseTargetFlags.Members)]
	private struct DeferredVertex
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector3 Tangent;
		public Vector3 BiTangent;
		public Vector2 Uv;
	}

	private readonly int elements;

	private readonly int vao;
	private readonly int vbo;
	private readonly int ebo;

	public unsafe Model(DeferredBuffer deferredBuffer, IReadOnlyList<Vertex> vertices, IReadOnlyCollection<Index> indices)
	{
		this.elements = indices.Count * 3;

		this.vao = GL.GenVertexArray();
		GL.BindVertexArray(this.vao);

		this.vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
		var vertexData = Model.BuildVertexData(vertices, indices);
		GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(DeferredVertex), vertexData, BufferUsageHint.StaticDraw);

		this.ebo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.ebo);
		var indexData = Enumerable.Range(0, this.elements).Select(i => (uint) i).ToArray();
		GL.BufferData(BufferTarget.ElementArrayBuffer, indexData.Length * sizeof(DeferredVertex), indexData, BufferUsageHint.StaticDraw);

		deferredBuffer.LayoutAttributes();

		GL.BindVertexArray(0);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
	}

	private static DeferredVertex[] BuildVertexData(IReadOnlyList<Vertex> vertices, IEnumerable<Index> indices)
	{
		var data = new List<DeferredVertex>();

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

			var biTangent = new Vector3(
				f * (-deltaUv2.X * edge1.X + deltaUv1.X * edge2.X),
				f * (-deltaUv2.X * edge1.Y + deltaUv1.X * edge2.Y),
				f * (-deltaUv2.X * edge1.Z + deltaUv1.X * edge2.Z)
			);

			data.Add(
				new DeferredVertex
				{
					Position = vertex1.Position,
					Normal = vertex1.Normal,
					Tangent = tangent,
					BiTangent = biTangent,
					Uv = vertex1.Uv
				}
			);

			data.Add(
				new DeferredVertex
				{
					Position = vertex2.Position,
					Normal = vertex2.Normal,
					Tangent = tangent,
					BiTangent = biTangent,
					Uv = vertex2.Uv
				}
			);

			data.Add(
				new DeferredVertex
				{
					Position = vertex3.Position,
					Normal = vertex3.Normal,
					Tangent = tangent,
					BiTangent = biTangent,
					Uv = vertex3.Uv
				}
			);
		}

		return data.ToArray();
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

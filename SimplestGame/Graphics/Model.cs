using OpenTK.Graphics.OpenGL4;

namespace SimplestGame.Graphics;

public class Model : IDisposable
{
    private int vertexBuffer;
    private int indexBuffer;
    private int vertexArray;

    private readonly Shader shader;
    private readonly int indices;

    public Model(float[] vertices, uint[] indices, Shader shader)
    {
        this.shader = shader;
        this.indices = indices.Length;

        //First VAO
        this.vertexArray = GL.GenVertexArray();
        GL.BindVertexArray(this.vertexArray);

        this.vertexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer); // open Bufferstream
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        //An EBO is a buffer, just like a vertex buffer object, that stores indices that OpenGL uses to decide what vertices to draw. 
        this.indexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBuffer); // open Bufferstream
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices,
            BufferUsageHint.StaticDraw);
        
        var position = GL.GetAttribLocation(shader.Program, "aPosition");
        GL.VertexAttribPointer(position, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(position);

        var texture = GL.GetAttribLocation(shader.Program, "aTexture");
        GL.VertexAttribPointer(texture, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(texture);

        //Unbinding
        //A VAO stores the glBindBuffer calls when the target is GL_ELEMENT_ARRAY_BUFFER.
        //This also means it stores its unbind calls so make sure you don't unbind the element array buffer before unbinding your VAO, otherwise it doesn't have an EBO configured.
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    public void Render()
    {
        GL.UseProgram(this.shader.Program);
        GL.BindVertexArray(this.vertexArray);

        GL.DrawElements(PrimitiveType.Triangles, this.indices, DrawElementsType.UnsignedInt, 0);

        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(this.vertexArray);
        GL.DeleteBuffer(this.indexBuffer);
        GL.DeleteBuffer(this.vertexBuffer);
    }
}
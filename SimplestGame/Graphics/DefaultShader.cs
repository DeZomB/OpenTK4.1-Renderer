using System.Net.Mime;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SimplestGame.Graphics;

public class DefaultShader : Shader
{
    // language=GLSL
    private const string VertexShader = @"
        #version 410 core
        
        uniform Uniforms
        {
            mat4 uView;
            mat4 uProjection;
        };
        
        uniform mat4 uModel;
            
        in vec3 aPosition;
        // We add another input variable for the texture coordinates:
        in vec2 aTexture;
        // ...However, they aren't needed for the vertex shader itself.
        // Instead, we create an output variable so we can send that data to the fragment shader.
        
        out vec2 vTexture;
        
        void main()
        {
            vTexture = aTexture;
            gl_Position = uProjection * uView *  vec4(aPosition, 1.0);
        }
    ";

    // language=GLSL
    private const string FragmentShader = @"
        #version 410 core 
        
        out vec4 fColor;
        
        uniform sampler2D uDiffuse;
        
        in vec2 vTexture;

        void main()
        {
            fColor = texture(uDiffuse, vTexture);
        }
    ";
    
    private struct Uniforms
    {
        public Matrix4 View;
        public Matrix4 Projection;
    }
    
    private readonly int uniformBuffer;
    private readonly int model;

    public unsafe DefaultShader() : base(VertexShader, FragmentShader)
    {
        GL.UniformBlockBinding(this.Program, GL.GetUniformBlockIndex(this.Program, "Uniforms"), 0);
        
        this.uniformBuffer = GL.GenBuffer();

        var uniforms = new Uniforms
        {
            View = Matrix4.Identity, 
            Projection = Matrix4.Identity
        };
        
        GL.BindBuffer(BufferTarget.UniformBuffer, this.uniformBuffer);
        GL.BufferData(BufferTarget.UniformBuffer, sizeof(Uniforms), ref uniforms, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.UniformBuffer,0);
        
        GL.BindBufferRange(BufferRangeTarget.UniformBuffer, 0, this.uniformBuffer, IntPtr.Zero, sizeof(Uniforms));

        this.model = GL.GetUniformLocation(this.Program, "uModel");
    }

    public unsafe void SetCamera(Camera camera)
    {
        var uniforms = new Uniforms
        {
            View = camera.View,
            Projection = camera.Projection
        };

        GL.BindBuffer(BufferTarget.UniformBuffer, this.uniformBuffer);
        GL.BufferData(BufferTarget.UniformBuffer, sizeof(Uniforms), ref uniforms, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.UniformBuffer,0);
    }

    public void SetModel(Matrix4 model)
    {
        GL.UniformMatrix4(this.model, false, ref model);
    }
    public override void Dispose()
    {
        GL.DeleteBuffer(this.uniformBuffer);
        base.Dispose();
    }
}
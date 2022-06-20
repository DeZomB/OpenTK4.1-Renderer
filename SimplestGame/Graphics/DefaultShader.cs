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
            mat4 uModel;
            mat4 uView;
            mat4 uProjection;
        };
            
        layout(location = 0) in vec3 aPosition;
        // We add another input variable for the texture coordinates:
        layout(location = 1) in vec2 aTexture;
        // ...However, they aren't needed for the vertex shader itself.
        // Instead, we create an output variable so we can send that data to the fragment shader.
        
        out vec2 vTexture;
        
        void main()
        {
            vTexture = aTexture;
            gl_Position = uProjection * uView * vec4(aPosition, 1.0);
        }
    ";

    // language=GLSL
    private const string FragmentShader = @"
        #version 410 core 
        
        out vec4 outputColor;
        
        uniform sampler2D uDiffuse;
        
        in vec2 vTexture;

        void main()
        {
            outputColor = texture(uDiffuse, vTexture);
        }
    ";
    
    private struct Uniforms
    {
        public Matrix4 Model;
        public Matrix4 View;
        public Matrix4 Projection;
    }
    
    private readonly int uniformBuffer;
    
    public unsafe DefaultShader() : base(VertexShader, FragmentShader)
    {
        GL.UniformBlockBinding(this.Program, GL.GetUniformBlockIndex(this.Program, "Uniforms"), 0);
        
        this.uniformBuffer = GL.GenBuffer();

        var uniforms = new Uniforms
        {
            Model = Matrix4.Identity,View = Matrix4.Identity, Projection = Matrix4.Identity
        };
        
        GL.BindBuffer(BufferTarget.UniformBuffer, this.uniformBuffer);
        GL.BufferData(BufferTarget.UniformBuffer, sizeof(Uniforms), ref uniforms, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.UniformBuffer,0);
        
        GL.BindBufferRange(BufferRangeTarget.UniformBuffer, 0, this.uniformBuffer, IntPtr.Zero, sizeof(Uniforms));
    }

    public unsafe void SetUniforms(Camera camera, double gameTime)
    {
        var uniforms = new Uniforms
        {
            Model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(gameTime)), View = camera.GetViewMatrix(), Projection = camera.GetProjectionMatrix()
            
        };

        GL.BindBuffer(BufferTarget.UniformBuffer, this.uniformBuffer);
        GL.BufferData(BufferTarget.UniformBuffer, sizeof(Uniforms), ref uniforms, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.UniformBuffer,0);
    }

    public override void Dispose()
    {
        GL.DeleteBuffer(this.uniformBuffer);
        base.Dispose();
    }
}
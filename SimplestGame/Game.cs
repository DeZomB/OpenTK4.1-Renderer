using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SimplestGame.Graphics;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace SimplestGame;

public class Game : GameWindow
{
    private readonly DefaultShader shader;
    private readonly Model model;
    private readonly Camera camera;
    private readonly ModelInstance modelInstance;
    private Texture texture;

    private double gameTime;
    private bool firstMouseMove = false;
    private Vector2 lastMousePosition;
    
    public Game() : base(new GameWindowSettings(),
        new NativeWindowSettings
        {
            Size = new Vector2i(800, 600),
            //Flags = ContextFlags.ForwardCompatible is needed for MacOS
            Profile = ContextProfile.Core, APIVersion = new Version(4, 1), Flags = ContextFlags.ForwardCompatible,
            Title = "Awesome Stuff"
        })
    {
        unsafe
        {
            GL.ClearColor(0, 0.5f, 0.75f, 1);

            var vertices = new []
            {
                // Position         Texture coordinates
                0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
                0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
                -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
                -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
            };


            var indices = new uint[]
            {
                0, 1, 3,
                1, 2, 3
            };

            this.shader = new DefaultShader();
            this.model = new Model(vertices, indices, this.shader);

        
            var image = Image.Load<Rgba32>(File.OpenRead("Assets/test.png"));
        
            //ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            var pixels = new byte[sizeof(Rgba32) * image.Width * image.Width];
            image.CopyPixelDataTo(pixels);
            
            this.texture = new Texture(pixels, image.Width, image.Height);
            this.modelInstance = new ModelInstance(this.model, this.texture);
        
            
            this.camera = new Camera(Vector3.UnitZ  * 3, Size.X/ (float)Size.Y);

            CursorState = CursorState.Grabbed;
        }
    }

    
    
    
    protected override void OnUnload() // At window close
    {
        this.shader.Dispose();
        this.model.Dispose();
        this.texture.Dispose();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        var input = KeyboardState;

        if (input.IsKeyDown(Keys.Escape))
        {
            Close();
        }
        
        const float cameraSpeed = 1.5f;
        const float sensitivity = 0.2f;

        if (input.IsKeyDown(Keys.W))
        {
            this.camera.cameraPosition += this.camera.Front * cameraSpeed * (float)args.Time; // Forward
        }

        if (input.IsKeyDown(Keys.S))
        {
            this.camera.cameraPosition -= this.camera.Front * cameraSpeed * (float)args.Time; // Backwards
        }
        if (input.IsKeyDown(Keys.A))
        {
            this.camera.cameraPosition -= this.camera.Right * cameraSpeed * (float)args.Time; // Left
        }
        if (input.IsKeyDown(Keys.D))
        {
            this.camera.cameraPosition += this.camera.Right * cameraSpeed * (float)args.Time; // Right
        }
        if (input.IsKeyDown(Keys.Space))
        {
            this.camera.cameraPosition += this.camera.Up * cameraSpeed * (float)args.Time; // Up
        }
        if (input.IsKeyDown(Keys.LeftShift))
        {
            this.camera.cameraPosition -= this.camera.Up * cameraSpeed * (float)args.Time; // Down
        }
        
        var mouse = MouseState;

        if (firstMouseMove) // This bool variable is initially set to true.
        {
            lastMousePosition = new Vector2(mouse.X, mouse.Y);
            firstMouseMove = false;
        }
        else
        {
            // Calculate the offset of the mouse position
            var deltaX = mouse.X - lastMousePosition.X;
            var deltaY = mouse.Y - lastMousePosition.Y;
            lastMousePosition = new Vector2(mouse.X, mouse.Y);

            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            this.camera.Yaw += deltaX * sensitivity;
            this.camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        gameTime += 4.0 * args.Time;
        
        GL.Viewport(0, 0, this.ClientSize.X, this.ClientSize.Y);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        
        this.shader.SetUniforms(this.camera, gameTime);

        
        this.modelInstance.Render(TextureUnit.Texture0);

        this.SwapBuffers();
    }
    
    protected override void OnMouseWheel(MouseWheelEventArgs args)
    {
        // In the mouse wheel function, we manage all the zooming of the camera.
        // This is simply done by changing the FOV of the camera.
        base.OnMouseWheel(args);

        this.camera.FOV -= args.OffsetY;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        this.camera.cameraAspectratio = Size.X / (float)Size.Y;
    }
}
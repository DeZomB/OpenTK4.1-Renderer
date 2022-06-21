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
    
    private bool firstMouseMove = true;
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

            var vertices = new[]
            {
                // Position         Texture coordinates
                5f, 5f, 0.0f, 1.0f, 1.0f, // top right
                5f, -5f, 0.0f, 1.0f, 0.0f, // bottom right
                -5f, -5f, 0.0f, 0.0f, 0.0f, // bottom left
                -5f, 5f, 0.0f, 0.0f, 1.0f // top left
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


            this.camera = new Camera()
            {
                Postion = Vector3.UnitZ * 10
            };


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
        CursorState = this.IsFocused ? CursorState.Grabbed : CursorState.Normal;

        ProcessMovement(args);
    }

    private void ProcessMovement(FrameEventArgs args)
    {
        var input = KeyboardState;

        if (input.IsKeyDown(Keys.Escape))
        {
            Close();
            return;
        }

        float cameraSpeed = 1.5f;
        const float sensitivity = 0.2f;

        if (input.IsKeyDown(Keys.LeftShift))
        {
            cameraSpeed *= 5;
        }
        if (input.IsKeyDown(Keys.W))
        {
            this.camera.Postion += this.camera.Forward * cameraSpeed * (float)args.Time; // Forward
        }

        if (input.IsKeyDown(Keys.S))
        {
            this.camera.Postion -= this.camera.Forward * cameraSpeed * (float)args.Time; // Backwards
        }

        if (input.IsKeyDown(Keys.A))
        {
            this.camera.Postion -= this.camera.Right * cameraSpeed * (float)args.Time; // Left
        }

        if (input.IsKeyDown(Keys.D))
        {
            this.camera.Postion += this.camera.Right * cameraSpeed * (float)args.Time; // Right
        }

        if (input.IsKeyDown(Keys.Space))
        {
            this.camera.Postion += Vector3.UnitY * cameraSpeed * (float)args.Time; // Up
        }

        if (input.IsKeyDown(Keys.LeftControl))
        {
            this.camera.Postion -= Vector3.UnitY * cameraSpeed * (float)args.Time; // Down
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
            this.camera.Pitch = Math.Clamp(this.camera.Pitch - deltaY * sensitivity, -89, 89); // Reversed since y-coordinates range from bottom to top
        }

        this.camera.FOV -= mouse.ScrollDelta.Y;
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Viewport(0, 0, this.ClientSize.X, this.ClientSize.Y);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        this.camera.Size = this.ClientSize;
        this.camera.Update();

        this.shader.SetCamera(this.camera);

        this.modelInstance.Render();

        this.SwapBuffers();
    }
}

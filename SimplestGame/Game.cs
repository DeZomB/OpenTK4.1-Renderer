namespace SimplestGame;

using EiveoEngine.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

public class Game : GameWindow
{
	private readonly DefaultShader shader;
	private readonly Model model;
	private readonly Texture texture;
	private readonly ModelInstance modelInstance;

	private readonly Camera camera;

	private bool skipMouseInput = true;

	public unsafe Game()
		: base(
			GameWindowSettings.Default,
			new NativeWindowSettings
			{
				Size = new Vector2i(800, 600),
				Title = "EiveoEngine",

				// The maximum supported OpenGL feature set under MacOS
				Profile = ContextProfile.Core,
				APIVersion = new Version(4, 1),
				Flags = ContextFlags.ForwardCompatible,
			}
		)
	{
		GL.ClearColor(0, 0.5f, 0.75f, 1);

		// TODO implement vertex structs.
		var vertices = new float[]
		{
			// vec3 uPosition
			// vec2 uTexture
			1, 1, 0, 1, 1,
			1, 0, 0, 1, 0,
			0, 0, 0, 0, 0,
			0, 1, 0, 0, 1
		};

		var indices = new uint[] { 0, 1, 3, 1, 2, 3 };

		var image = Image.Load<Rgba32>(File.OpenRead("Assets/test.png"));
		image.Mutate(x => x.Flip(FlipMode.Vertical));
		var pixels = new byte[image.Width * image.Width * sizeof(Rgba32)];
		image.CopyPixelDataTo(pixels);

		// TODO a model should be creatable without a shader.
		// TODO The shader assignment should be part of either the instance or be a parameter on rendering!
		this.shader = new DefaultShader();
		this.model = new Model(vertices, indices, this.shader);
		this.texture = new Texture(pixels, image.Width, image.Height);
		this.modelInstance = new ModelInstance(this.model, this.texture, this.shader) { Scale = new Vector3(50) };

		this.camera = new Camera { Postion = Vector3.UnitZ * 10 };
	}

	protected override void OnUnload()
	{
		this.shader.Dispose();
		this.model.Dispose();
		this.texture.Dispose();
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		if (this.IsFocused && this.CursorState != CursorState.Grabbed)
		{
			this.CursorState = CursorState.Grabbed;
			this.skipMouseInput = true;
		}
		else if (!this.IsFocused && this.CursorState == CursorState.Grabbed)
			this.CursorState = CursorState.Normal;

		this.ProcessMovement(args);
	}

	private void ProcessMovement(FrameEventArgs args)
	{
		if (this.KeyboardState.IsKeyDown(Keys.Escape))
		{
			this.Close();

			return;
		}

		var cameraSpeed = 1.5f;

		if (this.KeyboardState.IsKeyDown(Keys.LeftShift))
			cameraSpeed *= 5;

		if (this.KeyboardState.IsKeyDown(Keys.W))
			this.camera.Postion += this.camera.Forward * cameraSpeed * (float) args.Time;

		if (this.KeyboardState.IsKeyDown(Keys.S))
			this.camera.Postion -= this.camera.Forward * cameraSpeed * (float) args.Time;

		if (this.KeyboardState.IsKeyDown(Keys.A))
			this.camera.Postion -= this.camera.Right * cameraSpeed * (float) args.Time;

		if (this.KeyboardState.IsKeyDown(Keys.D))
			this.camera.Postion += this.camera.Right * cameraSpeed * (float) args.Time;

		if (this.KeyboardState.IsKeyDown(Keys.Space))
			this.camera.Postion += Vector3.UnitY * cameraSpeed * (float) args.Time;

		if (this.KeyboardState.IsKeyDown(Keys.LeftControl))
			this.camera.Postion -= Vector3.UnitY * cameraSpeed * (float) args.Time;


		if (this.skipMouseInput)
			this.skipMouseInput = false;
		else
		{
			const float sensitivity = 0.2f;

			this.camera.Yaw += this.MouseState.Delta.X * sensitivity;
			this.camera.Pitch = Math.Clamp(this.camera.Pitch - this.MouseState.Delta.Y * sensitivity, -89, 89);
		}

		this.camera.Fov -= this.MouseState.ScrollDelta.Y;
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		GL.Viewport(0, 0, this.ClientSize.X, this.ClientSize.Y);
		GL.Clear(ClearBufferMask.ColorBufferBit);

		this.camera.Update(this.ClientSize);
		this.shader.SetCamera(this.camera);

		this.modelInstance.Render();

		this.SwapBuffers();
	}
}

namespace SimplestGame;

using EiveoEngine.Extensions;
using EiveoEngine.Graphics;
using EiveoEngine.Graphics.Cameras;
using EiveoEngine.Graphics.Lights;
using EiveoEngine.Graphics.Renderer;
using EiveoEngine.Graphics.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Drawing;

public class Game : GameWindow
{
	private readonly DeferredRenderer deferredRenderer;
	private readonly Camera camera;
	private readonly Scene scene;

	private readonly ModelInstance lightSource;
	private readonly PointLight pointLight;

	private bool skipMouseInput = true;

	public Game()
		: base(
			GameWindowSettings.Default,

			// The maximum supported OpenGL feature set under MacOS
			new NativeWindowSettings { Flags = ContextFlags.ForwardCompatible, Profile = ContextProfile.Core, APIVersion = new(4, 6) }
		)
	{
		GL.Enable(EnableCap.DepthTest);
		GL.ClearColor(0, 0, 0, 1);

		this.deferredRenderer = new DeferredRenderer();
		this.camera = new PerspectiveCamera();

		var cubeModel = new Model(
			this.deferredRenderer.DeferredBuffer,
			new Vertex[]
			{
				new(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(0.0f, 0.0f)),
				new(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(1.0f, 1.0f)),
				new(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(1.0f, 0.0f)),
				new(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(1.0f, 1.0f)),
				new(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(0.0f, 0.0f)),
				new(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(0.0f, 1.0f)),
				new(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0, 0, 1), new Vector2(0.0f, 0.0f)),
				new(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0, 0, 1), new Vector2(1.0f, 0.0f)),
				new(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 0, 1), new Vector2(1.0f, 1.0f)),
				new(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 0, 1), new Vector2(1.0f, 1.0f)),
				new(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0, 0, 1), new Vector2(0.0f, 1.0f)),
				new(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0, 0, 1), new Vector2(0.0f, 0.0f)),
				new(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-1, 0, 0), new Vector2(1.0f, 0.0f)),
				new(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-1, 0, 0), new Vector2(1.0f, 1.0f)),
				new(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1, 0, 0), new Vector2(0.0f, 1.0f)),
				new(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1, 0, 0), new Vector2(0.0f, 1.0f)),
				new(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-1, 0, 0), new Vector2(0.0f, 0.0f)),
				new(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-1, 0, 0), new Vector2(1.0f, 0.0f)),
				new(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1, 0, 0), new Vector2(1.0f, 0.0f)),
				new(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(1, 0, 0), new Vector2(0.0f, 1.0f)),
				new(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(1, 0, 0), new Vector2(1.0f, 1.0f)),
				new(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(1, 0, 0), new Vector2(0.0f, 1.0f)),
				new(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1, 0, 0), new Vector2(1.0f, 0.0f)),
				new(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(1, 0, 0), new Vector2(0.0f, 0.0f)),
				new(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0), new Vector2(0.0f, 1.0f)),
				new(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0), new Vector2(1.0f, 1.0f)),
				new(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0, -1, 0), new Vector2(1.0f, 0.0f)),
				new(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0, -1, 0), new Vector2(1.0f, 0.0f)),
				new(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0, -1, 0), new Vector2(0.0f, 0.0f)),
				new(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0), new Vector2(0.0f, 1.0f)),
				new(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0, 1, 0), new Vector2(0.0f, 1.0f)),
				new(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 1, 0), new Vector2(1.0f, 0.0f)),
				new(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0, 1, 0), new Vector2(1.0f, 1.0f)),
				new(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 1, 0), new Vector2(1.0f, 0.0f)),
				new(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0, 1, 0), new Vector2(0.0f, 1.0f)),
				new(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0, 1, 0), new Vector2(0.0f, 0.0f))
			},
			new Index[]
			{
				new(0, 1, 2),
				new(3, 4, 5),
				new(6, 7, 8),
				new(9, 10, 11),
				new(12, 13, 14),
				new(15, 16, 17),
				new(18, 19, 20),
				new(21, 22, 23),
				new(24, 25, 26),
				new(27, 28, 29),
				new(30, 31, 32),
				new(33, 34, 35)
			}
		);

		var planeModel = new Model(
			this.deferredRenderer.DeferredBuffer,
			new Vertex[]
			{
				new(new Vector3(-0.5f, -0.5f, 0), new Vector3(0, 0, 1), new Vector2(0.0f, 0.0f)),
				new(new Vector3(0.5f, 0.5f, 0), new Vector3(0, 0, 1), new Vector2(1.0f, 1.0f)),
				new(new Vector3(0.5f, -0.5f, 0), new Vector3(0, 0, 1), new Vector2(1.0f, 0.0f)),
				new(new Vector3(0.5f, 0.5f, 0), new Vector3(0, 0, 1), new Vector2(1.0f, 1.0f)),
				new(new Vector3(-0.5f, -0.5f, 0), new Vector3(0, 0, 1), new Vector2(0.0f, 0.0f)),
				new(new Vector3(-0.5f, 0.5f, 0), new Vector3(0, 0, 1), new Vector2(0.0f, 1.0f))
			},
			new Index[] { new(0, 1, 2), new(3, 4, 5), new(0, 2, 1), new(3, 5, 4) }
		);

		var crateMaterial = new Material
		{
			AlbedoMap = new SimpleTexture("Assets/crate_d"),
			NormalMap = new SimpleTexture("Assets/crate_n"),
			SpecularMap = new SimpleTexture("Assets/crate_s")
		};

		var testMaterial = new Material
		{
			AlbedoMap = new SimpleTexture("Assets/crate_border_d"),
			NormalMap = new SimpleTexture("Assets/crate_border_n"),
			SpecularMap = new SimpleTexture("Assets/crate_border_s"),
			CubeMap = new CubeTexture("Assets/cloudy"),
			EmissiveMap = new SimpleTexture("Assets/spectrum_border")
		};

		var emissiveMaterial = new Material { EmissiveMap = new SimpleTexture("Assets/spectrum") };

		var lightMaterial = new Material { EmissiveColor = Color.White };

		this.scene = new Scene();

		for (var i = 0; i < 10000; i++)
		{
			this.scene.ModelInstances.Add(
				new ModelInstance(cubeModel, crateMaterial)
				{
					Position = new Vector3((int) (i / 100f), 0, i % 100) * 2, Rotation = new Vector3(1.0f, 0.3f, 0.5f) * (20f * i).ToRadians()
				}
			);
		}

		this.scene.ModelInstances.Add(new ModelInstance(cubeModel, testMaterial) { Position = new Vector3(0, 2, 0) });

		for (var i = 0; i < 10; i++)
			this.scene.ModelInstances.Add(new ModelInstance(planeModel, emissiveMaterial) { Position = new Vector3(2, 2, -i) });

		this.scene.ModelInstances.Add(
			this.lightSource = new ModelInstance(cubeModel, lightMaterial) { Scale = new Vector3(0.2f), Position = new Vector3(1.2f, 1.0f, 2.0f) }
		);

		this.scene.Lights.Add(new AmbientLight { Color = Color.FromArgb(15, 255, 255, 255) });
		this.scene.Lights.Add(new DirectionalLight { Color = Color.FromArgb(127, 255, 255, 255), Direction = -Vector3.UnitY });
		this.scene.Lights.Add(this.pointLight = new PointLight { Color = Color.FromArgb(255, 255, 0, 0) });
		this.scene.Lights.Add(new SpotLight { Color = Color.FromArgb(255, 0, 255, 0), Position = new Vector3(0, -5, 0), Direction = Vector3.UnitY });
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		if (this.IsFocused)
		{
			if (this.CursorState != CursorState.Grabbed && this.MouseState.IsButtonDown(MouseButton.Button1))
			{
				this.CursorState = CursorState.Grabbed;
				this.skipMouseInput = true;
			}
			else if (this.CursorState == CursorState.Grabbed && !this.MouseState.IsButtonDown(MouseButton.Button1))
				this.CursorState = CursorState.Normal;

			if (this.KeyboardState.IsKeyPressed(Keys.Q))
				this.deferredRenderer.Debug = !this.deferredRenderer.Debug;

			this.ProcessMovement(args);
		}
		else if (this.CursorState == CursorState.Grabbed)
			this.CursorState = CursorState.Normal;

		this.lightSource.Position = Vector3.Transform(this.lightSource.Position, Quaternion.FromEulerAngles((float) args.Time, 0, 0));
		this.pointLight.Position = this.lightSource.Position;
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

		if (this.skipMouseInput || this.CursorState != CursorState.Grabbed)
			this.skipMouseInput = false;
		else
		{
			const float sensitivity = 0.2f;

			this.camera.Yaw += this.MouseState.Delta.X * sensitivity;
			this.camera.Pitch = Math.Clamp(this.camera.Pitch - this.MouseState.Delta.Y * sensitivity, -89, 89);
		}

		if (this.camera is PerspectiveCamera perspectiveCamera)
			perspectiveCamera.Fov -= this.MouseState.ScrollDelta.Y;
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		GL.Viewport(0, 0, this.ClientSize.X, this.ClientSize.Y);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		this.camera.Update(this.ClientSize);

		this.deferredRenderer.Draw(this.camera, this.scene);

		stopwatch.Stop();
		this.Title = $"{1000 / stopwatch.ElapsedMilliseconds}";

		this.SwapBuffers();
	}
}

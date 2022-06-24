namespace EiveoEngine.Graphics.Renderer;

using Cameras;

public class DeferredRenderer
{
	public readonly DeferredBuffer DeferredBuffer;
	private readonly LightRenderer lightRenderer;
	private readonly ShadowRenderer shadowRenderer;
	private readonly DeferredDebugger deferredDebugger;
	private readonly DeferredOutput deferredOutput;

	public bool Debug = false;

	public DeferredRenderer()
	{
		this.DeferredBuffer = new DeferredBuffer();
		this.lightRenderer = new LightRenderer();
		this.shadowRenderer = new ShadowRenderer();
		this.deferredDebugger = new DeferredDebugger();
		this.deferredOutput = new DeferredOutput();
	}

	public void Draw(Camera camera, Scene scene)
	{
		this.DeferredBuffer.Resize((int) camera.Size.X, (int) camera.Size.Y);
		this.lightRenderer.Resize((int) camera.Size.X, (int) camera.Size.Y);
		this.shadowRenderer.Resize((int) camera.Size.X, (int) camera.Size.Y);

		this.DeferredBuffer.Draw(camera, scene);
		this.lightRenderer.Draw(this.DeferredBuffer, camera, scene);
		this.shadowRenderer.Draw(this.DeferredBuffer);

		if (this.Debug)
			this.deferredDebugger.Draw(this.DeferredBuffer, this.lightRenderer, this.shadowRenderer);
		else
			this.deferredOutput.Draw(this.DeferredBuffer, this.lightRenderer, this.shadowRenderer);
	}
}

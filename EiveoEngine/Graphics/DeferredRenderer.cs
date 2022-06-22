namespace EiveoEngine.Graphics;

using Cameras;

public class DeferredRenderer
{
	public readonly DeferredBuffer DeferredBuffer;
	private readonly DeferredDebugger deferredDebugger;
	private readonly LightRenderer lightRenderer;
	private readonly ShadowRenderer shadowRenderer;

	public DeferredRenderer()
	{
		this.DeferredBuffer = new DeferredBuffer();
		this.deferredDebugger = new DeferredDebugger();
		this.lightRenderer = new LightRenderer();
		this.shadowRenderer = new ShadowRenderer();
	}

	public void Draw(Camera camera, IEnumerable<ModelInstance> modelInstances)
	{
		this.DeferredBuffer.Resize((int) camera.Size.X, (int) camera.Size.Y);
		this.lightRenderer.Resize((int) camera.Size.X, (int) camera.Size.Y);
		this.shadowRenderer.Resize((int) camera.Size.X, (int) camera.Size.Y);
		
		this.DeferredBuffer.Draw(camera, modelInstances);
		this.lightRenderer.Draw(this.DeferredBuffer);
		this.shadowRenderer.Draw(this.DeferredBuffer);
		this.deferredDebugger.Draw(this.DeferredBuffer, this.lightRenderer, this.shadowRenderer);
	}
}

namespace EiveoEngine.Graphics;

using Cameras;

public class DeferredRenderer
{
	public readonly DeferredBuffer DeferredBuffer;
	private readonly DeferredDebugger deferredDebugger;

	public DeferredRenderer()
	{
		this.DeferredBuffer = new DeferredBuffer();
		this.deferredDebugger = new DeferredDebugger();
	}

	public void Draw(Camera camera, IEnumerable<ModelInstance> modelInstances)
	{
		this.DeferredBuffer.Resize((int) camera.Size.X, (int) camera.Size.Y);
		this.DeferredBuffer.Draw(camera, modelInstances);

		this.deferredDebugger.Draw(this.DeferredBuffer);
	}
}

namespace EiveoEngine.Graphics;

using Lights;

public class Scene
{
	public readonly List<ModelInstance> ModelInstances = new();
	public readonly List<Light> Lights = new();
}

namespace EiveoEngine.Graphics;

using Lights;

public class Scene
{
	public readonly List<ModelInstance> ModelInstances = new();
	public readonly List<AmbientLight> AmbientLights = new();
	public readonly List<DirectionalLight> DirectionalLights = new();
	public readonly List<PointLight> PointLights = new();
	public readonly List<SpotLight> SpotLights = new();
}

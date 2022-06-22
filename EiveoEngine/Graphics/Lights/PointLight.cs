namespace EiveoEngine.Graphics.Lights
{
	using OpenTK.Mathematics;

	public class PointLight : Light
	{
		public Vector3 Position;

		public float Constant = 1;
		public float Linear = 0.045f;
		public float Quadratic = 0.0075f;
	}
}

namespace EiveoEngine.Graphics.Lights
{
	using OpenTK.Mathematics;

	public class SpotLight : Light
	{
		public Vector3 Position;
		public Vector3 Direction;

		public float Constant = 1;
		public float Linear = 0.045f;
		public float Quadratic = 0.0075f;

		public float CutOffInner = 12.5f;
		public float CutOffOuter = 17.5f;
	}
}

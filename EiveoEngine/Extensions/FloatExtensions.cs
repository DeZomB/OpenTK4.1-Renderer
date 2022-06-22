namespace EiveoEngine.Extensions;

public static class FloatExtensions
{
	public static float ToRadians(this float value)
	{
		return (float) (Math.PI / 180 * value);
	}
}

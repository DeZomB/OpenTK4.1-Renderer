namespace EiveoEngine.Extensions;

using OpenTK.Mathematics;
using System.Drawing;

public static class ColorExtensions
{
	public static Vector4 ToVector4(this Color value)
	{
		return new Vector4(value.R, value.G, value.B, value.A) / byte.MaxValue;
	}
}

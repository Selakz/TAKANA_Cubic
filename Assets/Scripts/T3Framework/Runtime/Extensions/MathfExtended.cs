#nullable enable

using UnityEngine;

namespace T3Framework.Runtime.Extensions
{
	public static class MathfExtended
	{
		public static Vector2? GetIntersectionPoint2D
			(Vector2 direction1, Vector2 point1, Vector2 direction2, Vector2 point2)
		{
			float denominator = direction1.x * direction2.y - direction1.y * direction2.x;

			if (Mathf.Approximately(denominator, 0))
			{
				return null;
			}

			Vector2 diff = point2 - point1;
			float t = (diff.x * direction2.y - diff.y * direction2.x) / denominator;
			return point1 + t * direction1;
		}
	}
}
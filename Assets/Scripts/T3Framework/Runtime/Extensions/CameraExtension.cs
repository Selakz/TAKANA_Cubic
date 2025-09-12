using UnityEngine;

namespace T3Framework.Runtime.Extensions
{
	public static class CameraExtension
	{
		public static bool ContainsScreenPoint(this Camera camera, Vector3 screenPoint)
		{
			var normalizedScreenPoint = new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
			return camera.rect.Contains(normalizedScreenPoint);
		}
	}
}
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

		public static bool ScreenToWorldPoint
			(this Camera camera, Plane plane, Vector3 screenPoint, out Vector3 worldPoint)
		{
			Ray ray = camera.ScreenPointToRay(screenPoint);
			if (plane.Raycast(ray, out var distanceToPlane))
			{
				worldPoint = ray.GetPoint(distanceToPlane);
				return true;
			}

			worldPoint = Vector3.zero;
			return false;
		}
	}
}
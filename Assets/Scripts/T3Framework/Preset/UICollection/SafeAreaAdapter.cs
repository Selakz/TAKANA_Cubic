#nullable enable

using UnityEngine;

namespace T3Framework.Preset.UICollection
{
	[RequireComponent(typeof(RectTransform))]
	public class SafeAreaAdapter : MonoBehaviour
	{
		// System Functions
		void Start()
		{
			var rt = GetComponent<RectTransform>();
			var area = Screen.safeArea;

			var anchorMin = area.position;
			var anchorMax = area.position + area.size;

			anchorMin.x /= Screen.width;
			anchorMin.y /= Screen.height;
			anchorMax.x /= Screen.width;
			anchorMax.y /= Screen.height;

			rt.anchorMin = anchorMin;
			rt.anchorMax = anchorMax;
		}
	}
}
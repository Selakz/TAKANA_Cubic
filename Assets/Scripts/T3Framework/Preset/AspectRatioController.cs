#nullable enable

using UnityEngine;

namespace T3Framework.Preset
{
	public class AspectRatioController : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private float widthWeight;
		[SerializeField] private float heightWeight;
		[SerializeField] private Camera targetCamera = default!;

		// Defined Functions
		private void UpdateCameraViewport()
		{
			float targetAspect = widthWeight / heightWeight;
			float windowAspect = (float)Screen.width / Screen.height;
			float scaleHeight = windowAspect / targetAspect;

			if (scaleHeight < 1.0f)
			{
				Rect rect = targetCamera.rect;
				rect.width = 1.0f;
				rect.height = scaleHeight;
				rect.x = 0;
				rect.y = (1.0f - scaleHeight) / 2.0f;
				targetCamera.rect = rect;
			}
			else
			{
				float scaleWidth = 1.0f / scaleHeight;
				Rect rect = targetCamera.rect;
				rect.width = scaleWidth;
				rect.height = 1.0f;
				rect.x = (1.0f - scaleWidth) / 2.0f;
				rect.y = 0;
				targetCamera.rect = rect;
			}
		}

		// System Functions
		void Start()
		{
			UpdateCameraViewport();
		}
	}
}
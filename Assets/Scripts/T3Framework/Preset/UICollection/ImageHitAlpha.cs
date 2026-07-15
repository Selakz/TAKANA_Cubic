#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Preset.UICollection
{
	[RequireComponent(typeof(Image))]
	public class ImageHitAlpha : MonoBehaviour
	{
		[SerializeField] private float alphaThreshold = 0.1f;

		void Start()
		{
			Image image = GetComponent<Image>();
			image.alphaHitTestMinimumThreshold = alphaThreshold;
		}
	}
}
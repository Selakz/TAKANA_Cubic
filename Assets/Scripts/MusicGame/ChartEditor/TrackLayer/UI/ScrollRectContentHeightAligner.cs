#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.TrackLayer.UI
{
	public class ScrollRectContentHeightAligner : UIBehaviour
	{
		// Serializable and Public
		[SerializeField] private RectTransform self = default!;
		[SerializeField] private LayoutElement scrollRectLayoutElement = default!;
		[SerializeField] private float minHeight = 0;
		[SerializeField] private float maxHeight = 0;

		// Private

		// System Functions
		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			scrollRectLayoutElement.preferredHeight = Mathf.Clamp(self.rect.height, minHeight, maxHeight);
		}
	}
}
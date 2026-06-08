#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.TrackLayer.UI
{
	public class QuickLayerContent : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public TextMeshProUGUI NameText { get; set; } = default!;

		[field: SerializeField]
		public Image Indicator { get; set; } = default!;

		[field: SerializeField]
		public Image IsSelectedFrame { get; set; } = default!;
	}
}
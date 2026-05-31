#nullable enable

using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	public class TrackDecorator : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public SpriteRenderer Indicator { get; set; } = default!;

		[field: SerializeField]
		public SpriteRenderer EndIndicator { get; set; } = default!;
	}
}
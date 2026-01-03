#nullable enable

using UnityEngine;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackDecorator : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public SpriteRenderer Indicator { get; set; } = default!;
	}
}
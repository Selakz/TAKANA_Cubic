#nullable enable

using TMPro;
using UnityEngine;

namespace MusicGame.LevelResult.UI
{
	public class InfoHeaderView : MonoBehaviour
	{
		[field: SerializeField]
		public TextMeshProUGUI SongNameText { get; set; } = default!;

		[field: SerializeField]
		public TextMeshProUGUI DifficultyNameText { get; set; } = default!;

		[field: SerializeField]
		public TextMeshProUGUI DifficultyValueText { get; set; } = default!;
	}
}
#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.LevelSelect.UI
{
	public class SongInfoPanel : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Texture defaultCover = default!;
		[SerializeField] private RawImage coverImage = default!;
		[SerializeField] private TextMeshProUGUI? musicNameText = default!;
		[SerializeField] private TextMeshProUGUI? composerText = default!;
		[SerializeField] private TextMeshProUGUI? charterText = default!;
		[SerializeField] private TextMeshProUGUI? illustratorText = default!;
		[SerializeField] private TextMeshProUGUI? bpmDisplayText = default!;

		// Private
		private SongInfo? songInfo;

		// Defined Functions
		public void LoadCover(Texture? cover) => coverImage.LoadTextureCover(cover ?? defaultCover);

		public void LoadSongInfo(SongInfo? songInfo)
		{
			if (musicNameText != null) musicNameText.text = songInfo?.Title.Value ?? string.Empty;
			if (composerText != null) composerText.text = songInfo?.Composer.Value ?? string.Empty;
			if (illustratorText != null) illustratorText.text = songInfo?.Illustrator.Value ?? string.Empty;
			if (bpmDisplayText != null) bpmDisplayText.text = songInfo?.BpmDisplay ?? string.Empty;
		}

		public void LoadDifficulty(int difficulty)
		{
			DifficultyInfo? info = null;
			songInfo?.Difficulties.TryGetValue(difficulty, out info);
			if (charterText != null) charterText.text = info?.Charter.Value ?? string.Empty;
		}
	}
}
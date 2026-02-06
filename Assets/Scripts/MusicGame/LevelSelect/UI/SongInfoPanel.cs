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
		[SerializeField] private RawImage coverImage = default!;
		[SerializeField] private TextMeshProUGUI musicNameText = default!;
		[SerializeField] private TextMeshProUGUI composerText = default!;
		[SerializeField] private TextMeshProUGUI charterText = default!;
		[SerializeField] private TextMeshProUGUI illustratorText = default!;
		[SerializeField] private TextMeshProUGUI bpmDisplayText = default!;

		// Private
		private Texture defaultCover = default!;
		private SongInfo? songInfo;

		// Defined Functions
		public void LoadCover(Texture? cover) => coverImage.LoadTextureCover(cover ?? defaultCover);

		public void LoadSongInfo(SongInfo? songInfo)
		{
			musicNameText.text = songInfo?.Title.Value ?? string.Empty;
			composerText.text = songInfo?.Composer.Value ?? string.Empty;
			// illustratorText.text = songInfo?.Illustrator.Value ?? string.Empty;
			// bpmDisplayText.text = songInfo?.BpmDisplay ?? string.Empty;
		}

		public void LoadDifficulty(int difficulty)
		{
			DifficultyInfo? info = null;
			songInfo?.Difficulties.TryGetValue(difficulty, out info);
			charterText.text = info?.Charter.Value ?? string.Empty;
		}

		// System Functions
		void Awake()
		{
			defaultCover = coverImage.texture;
		}
	}
}
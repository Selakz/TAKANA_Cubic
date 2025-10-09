#nullable enable

using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Gameplay.Level.UI
{
	public class CoverLoader : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private RawImage coverImage = default!;

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			if (levelInfo.Cover != null) coverImage.texture = levelInfo.Cover;
		}

		// System Functions
		void OnEnable()
		{
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}
	}
}
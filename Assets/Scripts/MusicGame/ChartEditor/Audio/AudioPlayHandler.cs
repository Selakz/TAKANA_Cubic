using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MusicGame.ChartEditor.Audio
{
	public class AudioPlayHandler : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private LevelManager levelManager;

		// Defined Functions
		public void TogglePlay(InputAction.CallbackContext context)
		{
			EventManager.Instance.Invoke(levelManager.Music.IsPlaying ? "Level_OnPause" : "Level_OnResume");
		}

		public static void ForcePause(InputAction.CallbackContext context)
		{
			EventManager.Instance.Invoke("Level_OnPause");
		}

		// System Functions
		void OnEnable()
		{
			InputManager.Instance.Register("EditorBasic", "Pause", TogglePlay);
			InputManager.Instance.Register("EditorBasic", "ForcePause", ForcePause);
		}
	}
}
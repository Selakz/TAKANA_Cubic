#nullable enable

using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Setting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class TimeScrollPlugin : MonoBehaviour
	{
		// System Functions
		void Update()
		{
			// Manually judge
			if (!InputManager.Instance.GlobalInputEnabled || LevelManager.Instance.Music.Clip == null) return;

			float y = Mouse.current.scroll.ReadValue().y;
			if (y == 0) return;
			if (LevelManager.Instance.LevelCamera.ContainsScreenPoint(Input.mousePosition) &&
			    InScreenEditManager.Instance.TimeRetriever is GridTimeRetriever timeRetriever)
			{
				var current = LevelManager.Instance.Music.ChartTime;
				var scrollSensitivity = ISingletonSetting<EditorSetting>.Instance.ScrollSensitivity.Value;
				bool forward = y * scrollSensitivity > 0;
				scrollSensitivity = Mathf.Abs(scrollSensitivity);
				// Forced to have a liminality to prevent endless loop...
				for (int i = 0, liminal = 0; i < scrollSensitivity && liminal < 100; i++)
				{
					var next = forward ? timeRetriever.GetCeilTime(current) : timeRetriever.GetFloorTime(current);
					if (next <= 0 || next > LevelManager.Instance.Music.AudioLength)
					{
						current = next;
						break;
					}

					if (Mathf.Abs(next - current) <= 1)
					{
						i--;
						liminal++;
						current = next + (forward ? liminal : -liminal);
					}
					else
					{
						current = next;
					}
				}

				EventManager.Instance.Invoke("Level_OnReset", current);
			}
		}
	}
}
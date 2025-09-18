#nullable enable

using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
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
			if (LevelManager.Instance.Music.Clip == null) return;

			float y = Mouse.current.scroll.ReadValue().y;
			if (y == 0) return;
			if (LevelManager.Instance.LevelCamera.ContainsScreenPoint(Input.mousePosition) &&
			    InScreenEditManager.Instance.TimeRetriever is GridTimeRetriever timeRetriever)
			{
				var current = LevelManager.Instance.Music.ChartTime;
				var scrollSensitivity = ISingletonSetting<EditorSetting>.Instance.ScrollSensitivity;
				bool forward = y * scrollSensitivity > 0;
				scrollSensitivity = Mathf.Abs(scrollSensitivity);
				for (int i = 0; i < scrollSensitivity; i++)
				{
					var next = forward ? timeRetriever.GetCeilTime(current) : timeRetriever.GetFloorTime(current);
					if (next <= 0 || next > LevelManager.Instance.Music.AudioLength)
					{
						current = next;
						break;
					}

					if (Mathf.Abs(next - current) < 3) i--;
					current = next;
				}

				EventManager.Instance.Invoke("Level_OnReset", current);
			}
		}
	}
}
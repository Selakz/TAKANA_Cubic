#nullable enable

using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Audio;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class TimeScrollPlugin : HierarchySystem<TimeScrollPlugin>
	{
		// Serializable and Public
		[SerializeField] private float scrollTweenDuration = 0.1f;

		// Private
		[Inject, Key("stage")] private Camera levelCamera = default!;
		[Inject] private IGameAudioPlayer music = default!;
		[Inject] private GridTimeRetriever gridTimeRetriever = default!;
		
		// System Functions
		void Update()
		{
			float y = Mouse.current.scroll.ReadValue().y;
			if (y == 0) return;

			// Manually judge
			if (!ISingleton<InputManager>.Instance.GlobalInputEnabled ||
			    music.Clip == null ||
			    !levelCamera.ContainsScreenPoint(Mouse.current.position.ReadValue())) return;
			
			var current = music.ChartTime;
			var scrollSensitivity = ISingletonSetting<EditorSetting>.Instance.ScrollSensitivity.Value;
			bool forward = y * scrollSensitivity > 0;
			// Forced to have a liminality to prevent endless loop...
			for (int i = 0, liminal = 0; i < scrollSensitivity && liminal < 100; i++)
			{
				var next = forward
					? gridTimeRetriever.GetCeilTime(current)
					: gridTimeRetriever.GetFloorTime(current);
				if (next <= 0 || next > music.AudioLength)
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

			music.ChartTime = current;
		}
	}
}
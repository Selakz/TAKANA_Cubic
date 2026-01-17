#nullable enable

using System;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Audio;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class TimeScrollPlugin : MonoBehaviour, ISelfInstaller
	{
		// Private
		private Camera levelCamera = default!;
		private IGameAudioPlayer music = default!;
		private NotifiableProperty<ITimeRetriever> timeRetriever = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("stage")] Camera levelCamera,
			IGameAudioPlayer music,
			NotifiableProperty<ITimeRetriever> timeRetriever)
		{
			this.levelCamera = levelCamera;
			this.music = music;
			this.timeRetriever = timeRetriever;
		}

		public void SelfInstall(IContainerBuilder builder)
			=> builder.RegisterComponent(this).Keyed(Guid.NewGuid().ToString());

		// System Functions
		void Update()
		{
			float y = Mouse.current.scroll.ReadValue().y;
			if (y == 0) return;

			// Manually judge
			if (!ISingleton<InputManager>.Instance.GlobalInputEnabled || music.Clip == null) return;

			if (levelCamera.ContainsScreenPoint(Input.mousePosition) &&
			    timeRetriever.Value is GridTimeRetriever gridTimeRetriever)
			{
				var current = music.ChartTime;
				var scrollSensitivity = ISingletonSetting<EditorSetting>.Instance.ScrollSensitivity.Value;
				bool forward = y * scrollSensitivity > 0;
				scrollSensitivity = Mathf.Abs(scrollSensitivity);
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
}
#nullable enable

using MusicGame.Gameplay.Audio;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Level.UI
{
	public class TimingInputField : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField inputField = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputFieldRegistrar(inputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnInputFieldEndEdit)
		};

		// Private
		private GameAudioPlayer music = default!;

		// Defined Functions
		[Inject]
		private void Construct(GameAudioPlayer music) => this.music = music;

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void OnInputFieldEndEdit(string useless)
		{
			T3Time targetTime = int.Parse(inputField.text);
			music.ChartTime = targetTime;
		}

		// System Function
		void Update()
		{
			if (!inputField.isFocused)
			{
				inputField.SetTextWithoutNotify(music.ChartTime.ToString());
			}
		}
	}
}
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Audio.UI
{
	public class OffsetInputField : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField offsetInputField = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, (_, _) =>
			{
				var info = levelInfo.Value;
				if (info is not null) Offset = info.Chart.GetsOffsetInfo().Value;
				else Offset = 0;
			}),
			new InputFieldRegistrar(offsetInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
				OnOffsetInputFieldEndEdit)
		};

		// Private
		private GameAudioPlayer music = default!;
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		private T3Time Offset
		{
			set => offsetInputField.SetTextWithoutNotify(value.ToString());
		}

		// Defined Function
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo,
			GameAudioPlayer music)
		{
			this.levelInfo = levelInfo;
			this.music = music;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			offsetInputField.interactable = true;
			Offset = levelInfo.Chart.Properties.Get("offset", 0);
		}

		private void OnOffsetInputFieldEndEdit(string content)
		{
			if (int.TryParse(content, out int offset) && offset >= 0)
			{
				music.Offset = offset;
				if (levelInfo.Value?.Chart is { } chart) chart.SetOffsetInfo(offset);
			}
			else
			{
				Offset = music.Offset;
			}
		}
	}
}
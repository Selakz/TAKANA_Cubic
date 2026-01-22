#nullable enable

using System;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.EditorEntry.UI
{
	public class OpenEntryButton : HierarchySystem<OpenEntryButton>
	{
		// Serializable and Public
		[SerializeField] private Button openEntryButton = default!;
		[SerializeField] private Selectable backTarget = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () => backTarget.interactable = levelInfo.Value is not null),
			new ButtonRegistrar(openEntryButton, () =>
			{
				music.Pause();
				try
				{
					saver.SaveSettings();
					saver.SaveEditorChart();
				}
				catch (Exception ex)
				{
					Debug.Log(ex);
					T3Logger.Log("Notice", "App_SaveError", T3LogType.Error);
				}
			})
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private IGameAudioPlayer music = default!;
		private EditorLevelSaver saver = default!;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<LevelInfo?> levelInfo, IGameAudioPlayer music, EditorLevelSaver saver)
		{
			this.levelInfo = levelInfo;
			this.music = music;
			this.saver = saver;
		}
	}
}
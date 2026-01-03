#nullable enable

using System;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Level.UI
{
	public class SaveLevelButton : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Button saveEditingLevelButton = default!;
		[SerializeField] private Button savePlayableLevelButton = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(saveEditingLevelButton, OnSaveEditingLevelButtonClick),
			new ButtonRegistrar(savePlayableLevelButton, OnSavePlayableLevelButtonClick)
		};

		// Private
		private EditorLevelSaver levelSaver = default!;

		// Constructor
		[Inject]
		private void Construct(EditorLevelSaver levelSaver)
		{
			this.levelSaver = levelSaver;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		private void OnSaveEditingLevelButtonClick()
		{
			try
			{
				levelSaver.SaveSettings();
				levelSaver.SaveEditorChart();
			}
			catch (Exception ex)
			{
				Debug.Log(ex);
				T3Logger.Log("Notice", "App_SaveError", T3LogType.Error);
				return;
			}

			T3Logger.Log("Notice", "App_SaveComplete", T3LogType.Success);
		}

		private void OnSavePlayableLevelButtonClick()
		{
			try
			{
				levelSaver.SavePlayableChart();
			}
			catch (Exception ex)
			{
				Debug.Log(ex);
				T3Logger.Log("Notice", "App_SaveError", T3LogType.Error);
				return;
			}

			T3Logger.Log("Notice", "App_SaveGameChartComplete", T3LogType.Success);
		}
	}
}
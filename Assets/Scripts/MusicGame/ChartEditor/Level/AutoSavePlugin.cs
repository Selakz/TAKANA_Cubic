#nullable enable

using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Level
{
	public class AutoSavePlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				cts?.Cancel();
				cts?.Dispose();
				cts = null;

				if (levelInfo.Value is not null)
				{
					cts = new CancellationTokenSource();
					LoopAutoSave(cts.Token).Forget();
				}
			})
		};

		// Private
		private CancellationTokenSource? cts;
		private EditorLevelSaver editorLevelSaver = default!;
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		// Constructor
		[Inject]
		private void Construct(
			EditorLevelSaver editorLevelSaver,
			NotifiableProperty<LevelInfo?> levelInfo)
		{
			this.editorLevelSaver = editorLevelSaver;
			this.levelInfo = levelInfo;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		private async UniTaskVoid LoopAutoSave(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				var delay = ISingleton<EditorSetting>.Instance.AutoSaveInterval.Value.Milli;
				await UniTask.Delay(delay, cancellationToken: token);
				AutoSave();
			}
		}

		private void AutoSave()
		{
			Debug.Log("AutoSave");
			if (levelInfo.Value is not { } info) return;
			var folderPath = FileHelper.GetAbsolutePathFromRelative(info.LevelPath, "AutoSave");
			Directory.CreateDirectory(folderPath);
			T3ProjSetting projectSetting = ISetting<T3ProjSetting>.Load(info.LevelPath);
			var fileName =
				$"{DateTime.Now:yyyy-MM-dd_HH-mm}_{projectSetting.GetChartFileName(info.Difficulty)}.editing.json";
			editorLevelSaver.SaveEditorChart(Path.Combine(folderPath, fileName));
			editorLevelSaver.SaveSettings();
		}
	}
}
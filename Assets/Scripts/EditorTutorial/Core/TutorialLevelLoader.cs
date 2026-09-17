#nullable enable

using System.Linq;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using Yarn.Unity;

namespace MusicGame.EditorTutorial
{
	public class TutorialLevelLoader : HierarchySystem<TutorialLevelLoader>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new[]
		{
			dispatcher.Registrar("LoadLevel", () =>
			{
				try
				{
					saver.SaveSettings();
					saver.SaveEditorChart();
				}
				catch
				{
					Debug.LogError("Save level failed");
				}

				timeRetriever.Value = defaultTimeRetriever;
				var clip = Resources.Load<AudioClip>("Tutorial/BuiltInLevel/music");
				var chartText = Resources.Load<TextAsset>("Tutorial/BuiltInLevel/chart").text;
				var chart = ChartInfo.Deserialize(JObject.Parse(chartText));
				var songInfoText = Resources.Load<TextAsset>("Tutorial/BuiltInLevel/songinfo").text;
				var songInfo = SettingConvertHelper.Deserializer.Deserialize<SongInfo>(songInfoText);
				var preferenceText = Resources.Load<TextAsset>("Tutorial/BuiltInLevel/preference").text;
				var preference = SettingConvertHelper.Deserializer.Deserialize<EditorPreference>(preferenceText);
				var info = new LevelInfo
				{
					LevelPath = "BuiltInResource",
					Chart = chart,
					Music = clip,
					SongInfo = songInfo,
					Preference = preference,
					Difficulty = preference.Difficulty
				};
				levelInfo.Value = info;
				return YarnTask.CompletedTask;
			}),
			dialogueRunner.Registrar("replace", Replace),
			dialogueRunner.Registrar("append", Append),
			dialogueRunner.Registrar("time", SetTime),
		};

		// Private
		[Inject] private readonly TutorialTaskDispatcher dispatcher = default!;
		[Inject] private readonly DialogueRunner dialogueRunner = default!;
		[Inject] private readonly NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private readonly IGameAudioPlayer music = default!;
		[Inject] private readonly EditorLevelSaver saver = default!;
		[Inject] private readonly NotifiableProperty<ITimeRetriever> timeRetriever = default!;
		[Inject] private readonly ITimeRetriever defaultTimeRetriever = default!;

		// Defined Functions
		public YarnTask Replace(string chartName)
		{
			if (levelInfo.Value is null) return YarnTask.CompletedTask;
			var chart = GetTutorialChart(chartName);
			levelInfo.Value!.Chart = chart;
			levelInfo.ForceNotify();
			return YarnTask.CompletedTask;
		}

		public YarnTask Append(string chartName)
		{
			if (levelInfo.Value is null) return YarnTask.CompletedTask;
			var chart = GetTutorialChart(chartName).ToArray();
			foreach (var component in chart)
			{
				component.BelongingChart = levelInfo.Value.Chart;
			}

			return YarnTask.CompletedTask;
		}

		public YarnTask SetTime(int time)
		{
			music.ChartTime = time;
			return YarnTask.CompletedTask;
		}

		private static ChartInfo GetTutorialChart(string chartName)
		{
			var chartText = Resources.Load<TextAsset>($"Tutorial/BuiltInLevel/{chartName}").text;
			var chart = ChartInfo.Deserialize(JObject.Parse(chartText));
			return chart;
		}
	}
}
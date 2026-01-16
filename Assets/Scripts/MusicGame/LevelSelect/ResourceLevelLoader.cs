#nullable enable

using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.LevelSelect
{
	public class ResourceLevelLoader : T3MonoBehaviour, ISelfInstaller
	{
		// Private
		private IDataset<LevelComponent> dataset = default!;

		// Constructor
		[Inject]
		private void Construct(IDataset<LevelComponent> dataset)
		{
			this.dataset = dataset;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		async void Start()
		{
			var music = Resources.Load<AudioClip>("Android/Test/music");
			var chartText = Resources.Load<TextAsset>("Android/Test/chart").text;
			var chart = ChartInfo.Deserialize(JObject.Parse(chartText));
			var songInfoText = Resources.Load<TextAsset>("Android/Test/songInfo").text;
			var songInfo = SettingConvertHelper.Deserializer.Deserialize<SongInfo>(songInfoText);
			var preferenceText = Resources.Load<TextAsset>("Android/Test/preference").text;
			var preference = SettingConvertHelper.Deserializer.Deserialize<GameplayPreference>(preferenceText);
			var levelInfo = new LevelInfo
			{
				LevelPath = "projectSettingPath",
				Chart = chart,
				Music = music,
				SongInfo = songInfo,
				Preference = preference,
				Difficulty = preference.Difficulty
			};
			var rawLevelInfo = await RawLevelInfo.FromLevelInfo(levelInfo);
			dataset.Add(new(rawLevelInfo));
		}
	}
}
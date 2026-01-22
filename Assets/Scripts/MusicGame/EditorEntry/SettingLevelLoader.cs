#nullable enable

using System.IO;
using MusicGame.ChartEditor.Level;
using MusicGame.LevelSelect;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.EditorEntry
{
	public class SettingLevelLoader : HierarchySystem<SettingLevelLoader>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DatasetRegistrar<LevelComponent<EditorPreference>>(dataset,
				DatasetRegistrar<LevelComponent<EditorPreference>>.RegisterTarget.DataAdded,
				data =>
				{
					var list = ISingletonSetting<EditorSetting>.Instance.RecentProjects.Value;
					if (Path.GetDirectoryName(data.Model.LevelPath) is { } directory && !list.Contains(directory))
					{
						list.Add(directory);
						ISingletonSetting<EditorSetting>.SaveInstance();
					}
				}),
			new ListDatasetOrderRegistrar<LevelComponent<EditorPreference>>(dataset, SyncDatasetToSetting)
		};

		// Private
		private ListDataset<LevelComponent<EditorPreference>> dataset = default!;

		// Constructor
		[Inject]
		private void Construct(ListDataset<LevelComponent<EditorPreference>> dataset)
		{
			this.dataset = dataset;
		}

		// Defined Functions
		public void SyncDatasetToSetting()
		{
			var list = ISingletonSetting<EditorSetting>.Instance.RecentProjects.Value;
			int count = ISingletonSetting<EditorSetting>.Instance.RecentProjectBufferCount;
			list.Clear();
			foreach (var data in dataset)
			{
				if (Path.GetDirectoryName(data.Model.LevelPath) is { } directory) list.Add(directory);
				if (--count <= 0) break;
			}

			ISingletonSetting<EditorSetting>.SaveInstance();
		}

		// System Functions
		async void Start()
		{
			int count = ISingletonSetting<EditorSetting>.Instance.RecentProjectBufferCount;
			foreach (var projectPath in ISingletonSetting<EditorSetting>.Instance.RecentProjects.Value)
			{
				var info = await RawLevelInfo<EditorPreference>.FromFolder(projectPath);
				if (info is not null) dataset.Add(new(info));
				if (--count <= 0) break;
			}
		}
	}
}
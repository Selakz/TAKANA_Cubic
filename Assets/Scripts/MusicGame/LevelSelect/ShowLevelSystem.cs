#nullable enable

using MusicGame.Gameplay.Level;
using MusicGame.LevelSelect.UI;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.LevelSelect
{
	public class ShowLevelSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private ViewPoolInstaller levelPanelInstaller;
		[SerializeField] private DifficultyConfig difficultyConfig = default!;
		[SerializeField] private GameObject loadingPanel = default!;
		[SerializeField] private SongInfoPanel songInfoPanel = default!;
		[SerializeField] private int playfieldSceneIndex;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<RawLevelInfo<GameplayPreference>?>(levelInfo, info =>
			{
				if (info is null) return;
				songInfoPanel.LoadCover(info.Cover.Value);
				songInfoPanel.LoadSongInfo(info.SongInfo.Value);
			}),
			new DatasetRegistrar<LevelComponent<GameplayPreference>>(dataset,
				DatasetRegistrar<LevelComponent<GameplayPreference>>.RegisterTarget.DataAdded,
				component =>
				{
					viewPool.Add(component);
					if (dataset.Count == 1) levelInfo.Value = component.Model;
				}),
			new DatasetRegistrar<LevelComponent<GameplayPreference>>(dataset,
				DatasetRegistrar<LevelComponent<GameplayPreference>>.RegisterTarget.DataRemoved,
				component => viewPool.Remove(component)),
			new ViewPoolLifetimeRegistrar<LevelComponent<GameplayPreference>>(viewPool,
				handler => new LevelPanelRegistrar(handler.Script<LevelPanel>(), levelInfo, difficulty,
					viewPool[handler]!, playfieldSceneIndex, difficultyConfig, loadingPanel))
		};

		// Private
		private NotifiableProperty<RawLevelInfo<GameplayPreference>?> levelInfo = default!;
		private NotifiableProperty<int> difficulty = default!;
		private IDataset<LevelComponent<GameplayPreference>> dataset = default!;
		private IViewPool<LevelComponent<GameplayPreference>> viewPool = default!;

		// Constructor
		[Inject]
		private void Construct(
			NotifiableProperty<RawLevelInfo<GameplayPreference>?> levelInfo,
			NotifiableProperty<int> difficulty,
			IDataset<LevelComponent<GameplayPreference>> dataset,
			IViewPool<LevelComponent<GameplayPreference>> viewPool)
		{
			this.levelInfo = levelInfo;
			this.difficulty = difficulty;
			this.dataset = dataset;
			this.viewPool = viewPool;
		}

		public void SelfInstall(IContainerBuilder builder)
		{
			levelPanelInstaller.Register<ViewPool<LevelComponent<GameplayPreference>>,
				LevelComponent<GameplayPreference>>(builder, Lifetime.Singleton);
			builder.RegisterComponent(this);
		}
	}
}
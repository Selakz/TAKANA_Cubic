#nullable enable

using MusicGame.Gameplay.Level;
using MusicGame.LevelSelect.UI;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
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
		[SerializeField] private int playfieldSceneIndex;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DatasetRegistrar<LevelComponent<GameplayPreference>>(dataset,
				DatasetRegistrar<LevelComponent<GameplayPreference>>.RegisterTarget.DataAdded,
				component => viewPool.Add(component)),
			new DatasetRegistrar<LevelComponent<GameplayPreference>>(dataset,
				DatasetRegistrar<LevelComponent<GameplayPreference>>.RegisterTarget.DataRemoved,
				component => viewPool.Remove(component)),
			new ViewPoolLifetimeRegistrar<LevelComponent<GameplayPreference>>(viewPool,
				handler => new LevelPanelRegistrar(handler.Script<LevelPanel>(),
					viewPool[handler]!, playfieldSceneIndex, difficultyConfig, loadingPanel))
		};

		// Private
		private IDataset<LevelComponent<GameplayPreference>> dataset = default!;
		private IViewPool<LevelComponent<GameplayPreference>> viewPool = default!;

		// Constructor
		[Inject]
		private void Construct(
			IDataset<LevelComponent<GameplayPreference>> dataset,
			IViewPool<LevelComponent<GameplayPreference>> viewPool)
		{
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
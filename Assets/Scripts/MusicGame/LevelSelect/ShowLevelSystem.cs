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
			new DatasetRegistrar<LevelComponent>(dataset,
				DatasetRegistrar<LevelComponent>.RegisterTarget.DataAdded,
				component => viewPool.Add(component)),
			new DatasetRegistrar<LevelComponent>(dataset,
				DatasetRegistrar<LevelComponent>.RegisterTarget.DataRemoved,
				component => viewPool.Remove(component)),
			new ViewPoolLifetimeRegistrar<LevelComponent>(viewPool,
				handler => new LevelPanelRegistrar(handler.Script<LevelPanel>(),
					viewPool[handler]!, playfieldSceneIndex, difficultyConfig, loadingPanel))
		};

		// Private
		private IDataset<LevelComponent> dataset = default!;
		private IViewPool<LevelComponent> viewPool = default!;

		// Constructor
		[Inject]
		private void Construct(
			IDataset<LevelComponent> dataset,
			IViewPool<LevelComponent> viewPool)
		{
			this.dataset = dataset;
			this.viewPool = viewPool;
		}

		public void SelfInstall(IContainerBuilder builder)
		{
			levelPanelInstaller.Register<ViewPool<LevelComponent>, LevelComponent>(builder, Lifetime.Singleton);
			builder.RegisterComponent(this);
		}
	}
}
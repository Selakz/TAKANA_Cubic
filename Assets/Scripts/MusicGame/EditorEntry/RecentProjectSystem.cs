#nullable enable

using MusicGame.ChartEditor.Level;
using MusicGame.EditorEntry.UI;
using MusicGame.Gameplay.Level;
using MusicGame.LevelSelect;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.EditorEntry
{
	public class RecentProjectSystem : HierarchySystem<RecentProjectSystem>
	{
		// Serializable and Public
		[SerializeField] private GameObject entryCanvas = default!;
		[SerializeField] private ViewPoolInstaller projectPanelInstaller;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolDataRegistrar<LevelComponent<EditorPreference>>(dataset, viewPool),
			new ViewPoolLifetimeRegistrar<LevelComponent<EditorPreference>>(viewPool, handler =>
				new ProjectItemRegistrar(handler.Script<ProjectItem>(),
					viewPool[handler]!, dataset, levelInfo, entryCanvas)),
			new ListDatasetViewSorter<LevelComponent<EditorPreference>>(dataset, viewPool),
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				if (levelInfo.Value is not null)
				{
					entryCanvas.SetActive(false);
					ISingleton<InputManager>.Instance.GlobalInputEnabled.Value = true;
				}
			})
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private ListDataset<LevelComponent<EditorPreference>> dataset = default!;
		private IViewPool<LevelComponent<EditorPreference>> viewPool = default!;

		// Constructor
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo,
			ListDataset<LevelComponent<EditorPreference>> dataset,
			IViewPool<LevelComponent<EditorPreference>> viewPool)
		{
			this.levelInfo = levelInfo;
			this.dataset = dataset;
			this.viewPool = viewPool;
		}

		public override void SelfInstall(IContainerBuilder builder)
		{
			base.SelfInstall(builder);
			projectPanelInstaller.Register<ViewPool<LevelComponent<EditorPreference>>, LevelComponent<EditorPreference>>
				(builder, Lifetime.Singleton);
		}
	}
}
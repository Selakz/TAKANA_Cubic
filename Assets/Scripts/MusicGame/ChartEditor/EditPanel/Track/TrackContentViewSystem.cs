#nullable enable

using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class TrackContentViewSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private float singleHeight = 600;

		// Private
		private IViewPool<ChartComponent> decoratorPool = default!;
		private IViewPool<EdgeComponent> edgePool = default!;
		private IViewPool<EdgePMLComponent> edgePmlPool = default!;
		private IViewPool<EdgeNodeComponent> edgeNodePool = default!;
		private ChartSelectDataset selectDataset = default!;
		private TrackLayerManageSystem trackLayerManageSystem = default!;
		private CommandManager commandManager = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(decoratorPool, handler => new TrackContentRegistrar(
				handler.Script<EditTrackContent>(), decoratorPool[handler]!, selectDataset, trackLayerManageSystem)),
			// new ViewPoolLifetimeRegistrar<EdgeComponent>(edgePool, handler => new EdgeMovementContentRegistrar()),
			new ViewPoolLifetimeRegistrar<EdgePMLComponent>(edgePmlPool, handler => new MoveListContentRegistrar(
				handler.Script<EditMoveListContent>())),
			new ViewPoolLifetimeRegistrar<EdgeNodeComponent>(edgeNodePool, handler => new V1EItemContentRegistrar(
				handler.Script<EditV1EItemContent>(), edgeNodePool[handler]!, commandManager)),

			new ViewPoolRegistrar<EdgePMLComponent>(edgePmlPool,
				ViewPoolRegistrar<EdgePMLComponent>.RegisterTarget.Get,
				handler =>
				{
					if (decoratorPool.Count == 1)
					{
						var content = handler.Script<EditMoveListContent>();
						content.HeightModifier.Assign(singleHeight, 1);
					}
					else
					{
						foreach (var data in edgePmlPool)
						{
							var content = edgePmlPool[data]!.Script<EditMoveListContent>();
							content.HeightModifier.Unregister(1);
						}
					}
				})
		};

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("track-decoration")] IViewPool<ChartComponent> decoratorPool,
			IViewPool<EdgeComponent> edgePool,
			IViewPool<EdgePMLComponent> edgePmlPool,
			IViewPool<EdgeNodeComponent> edgeNodePool,
			ChartSelectDataset selectDataset,
			TrackLayerManageSystem trackLayerManageSystem,
			CommandManager commandManager)
		{
			this.decoratorPool = decoratorPool;
			this.edgePool = edgePool;
			this.edgePmlPool = edgePmlPool;
			this.edgeNodePool = edgeNodePool;
			this.selectDataset = selectDataset;
			this.trackLayerManageSystem = trackLayerManageSystem;
			this.commandManager = commandManager;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}
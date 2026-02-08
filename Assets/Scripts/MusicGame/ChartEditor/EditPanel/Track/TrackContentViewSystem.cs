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
		private IViewPool<DirectComponent> directPool = default!;
		private IViewPool<DirectPMLComponent> directPmlPool = default!;
		private IViewPool<DirectNodeComponent> directNodePool = default!;
		private ChartSelectDataset selectDataset = default!;
		private TrackLayerManageSystem trackLayerManageSystem = default!;
		private CommandManager commandManager = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(decoratorPool, handler => new TrackContentRegistrar(
				handler.Script<EditTrackContent>(), decoratorPool[handler]!, selectDataset, trackLayerManageSystem)),
			// Edge Movement
			// new ViewPoolLifetimeRegistrar<EdgeComponent>(edgePool, handler => new EdgeMovementContentRegistrar()),
			new ViewPoolLifetimeRegistrar<EdgePMLComponent>(edgePmlPool, handler => new MoveListContentRegistrar(
				handler.Script<EditMoveListContent>())),
			new ViewPoolLifetimeRegistrar<EdgeNodeComponent>(edgeNodePool, EdgeNodeRegistrarFactory),
			// Direct Movement
			// new ViewPoolLifetimeRegistrar<DirectComponent>(directPool, handler => new EdgeMovementContentRegistrar()),
			new ViewPoolLifetimeRegistrar<DirectPMLComponent>(directPmlPool, handler => new MoveListContentRegistrar(
				handler.Script<EditMoveListContent>())),
			new ViewPoolLifetimeRegistrar<DirectNodeComponent>(directNodePool, DirectNodeRegistrarFactory),
			// Add Height
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
				}),
			new ViewPoolRegistrar<DirectPMLComponent>(directPmlPool,
				ViewPoolRegistrar<DirectPMLComponent>.RegisterTarget.Get,
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
			IViewPool<DirectComponent> directPool,
			IViewPool<DirectPMLComponent> directPmlPool,
			IViewPool<DirectNodeComponent> directNodePool,
			ChartSelectDataset selectDataset,
			TrackLayerManageSystem trackLayerManageSystem,
			CommandManager commandManager)
		{
			this.decoratorPool = decoratorPool;
			this.edgePool = edgePool;
			this.edgePmlPool = edgePmlPool;
			this.edgeNodePool = edgeNodePool;
			this.directPool = directPool;
			this.directPmlPool = directPmlPool;
			this.directNodePool = directNodePool;
			this.selectDataset = selectDataset;
			this.trackLayerManageSystem = trackLayerManageSystem;
			this.commandManager = commandManager;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		private MoveItemContentRegistrar EdgeNodeRegistrarFactory(PrefabHandler handler)
		{
			var component = edgeNodePool[handler]!;
			var content = handler.Script<IEditMoveItemContent>();
			return content switch
			{
				EditV1EItemContent eContent => new V1EItemContentRegistrar(eContent, component.Locator.Track,
					component.Locator.Time, component, component.Locator.IsLeft, commandManager, true),
				EditV1BItemContent bContent => new V1BItemContentRegistrar(bContent, component.Locator.Track,
					component.Locator.Time, component, component.Locator.IsLeft, commandManager, true),
				_ => null!
			};
		}

		private MoveItemContentRegistrar DirectNodeRegistrarFactory(PrefabHandler handler)
		{
			var component = directNodePool[handler]!;
			var content = handler.Script<IEditMoveItemContent>();
			return content switch
			{
				EditV1EItemContent eContent => new V1EItemContentRegistrar(eContent, component.Locator.Track,
					component.Locator.Time, component, component.Locator.IsPos, commandManager, false),
				EditV1BItemContent bContent => new V1BItemContentRegistrar(bContent, component.Locator.Track,
					component.Locator.Time, component, component.Locator.IsPos, commandManager, false),
				_ => null!
			};
		}
	}
}
#nullable enable

using System;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class TrackContentViewSystem : HierarchySystem<TrackContentViewSystem>
	{
		// Serializable and Public
		[SerializeField] private float singleHeight = 600;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(decoratorPool, handler => new TrackContentRegistrar(
				handler.Script<EditTrackContent>(), decoratorPool[handler]!, selectDataset, trackLayerManageSystem)),
			new ViewPoolLifetimeRegistrar<TrackNodeComponent>(nodePool, NodeRegistrarFactory),
			new DatasetRegistrar<ChartComponent>(decoratorPool,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataAdded,
				track =>
				{
					UpdateMoveListHeights();
					SortTrackMoveLists(track);
				}),
			new DatasetRegistrar<ChartComponent>(decoratorPool,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataRemoved,
				track => UpdateMoveListHeights(track)),
			CustomRegistrar.Generic<Action<ChartComponent>>(
				e => nodeDataset.OnTrackSynced += e,
				e => nodeDataset.OnTrackSynced -= e,
				SortTrackMoveLists)
		};

		// Private
		[Inject] [Key("track-decoration")] private readonly IViewPool<ChartComponent> decoratorPool = default!;
		[Inject] private readonly IViewPool<TrackNodeComponent> nodePool = default!;
		[Inject] private readonly TrackNodeDataset nodeDataset = default!;
		[Inject] private readonly ChartSelectDataset selectDataset = default!;
		[Inject] private readonly TrackLayerManageSystem trackLayerManageSystem = default!;
		[Inject] private readonly CommandManager commandManager = default!;

		// Defined Functions
		private MoveItemContentRegistrar NodeRegistrarFactory(PrefabHandler handler)
		{
			var node = nodePool[handler]!;
			var content = handler.Script<IEditMoveItemContent>();
			return content switch
			{
				EditV1EItemContent eContent => new V1EItemContentRegistrar(eContent, node, commandManager),
				EditV1BItemContent bContent => new V1BItemContentRegistrar(bContent, node, commandManager),
				_ => null!
			};
		}

		private void UpdateMoveListHeights(ChartComponent? removing = null)
		{
			var activeCount = removing is null ? decoratorPool.Count : decoratorPool.Count - 1;
			foreach (var track in decoratorPool)
			{
				if (track == removing || decoratorPool[track] is not { } handler) continue;
				var movement = handler.Script<EditTrackContent>().MovementContent;
				if (activeCount == 1)
				{
					movement.MoveList1.HeightModifier.Assign(singleHeight, 1);
					movement.MoveList2.HeightModifier.Assign(singleHeight, 1);
				}
				else
				{
					movement.MoveList1.HeightModifier.Unregister(1);
					movement.MoveList2.HeightModifier.Unregister(1);
				}
			}
		}

		private void SortTrackMoveLists(ChartComponent track)
		{
			if (decoratorPool[track] is not { } handler) return;
			var movement = handler.Script<EditTrackContent>().MovementContent;
			movement.MoveList1.SortListItems(CompareNodeHandler);
			movement.MoveList2.SortListItems(CompareNodeHandler);
			return;

			int CompareNodeHandler(PrefabHandler a, PrefabHandler b)
			{
				var timeA = nodePool[a]?.Time.Value ?? T3Time.MinValue;
				var timeB = nodePool[b]?.Time.Value ?? T3Time.MinValue;
				return timeA.CompareTo(timeB);
			}
		}
	}
}

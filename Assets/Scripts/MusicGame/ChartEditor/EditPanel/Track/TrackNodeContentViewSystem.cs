#nullable enable

using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.TrackLine;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class TrackNodeContentViewSystem : HierarchySystem<TrackNodeContentViewSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DatasetRegistrar<TrackNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataAdded,
				OnNodeSelectedOrUpdated),
			new DatasetRegistrar<TrackNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataUpdated,
				OnNodeSelectedOrUpdated),
			new DatasetRegistrar<TrackNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataRemoved,
				node =>
				{
					if (nodePool[node] is not { } handler) return;
					var content = handler.Script<IEditMoveItemContent>();
					content.BgColorModifier.Unregister(1);
				}),
			new ViewPoolRegistrar<TrackNodeComponent>(nodePool,
				ViewPoolRegistrar<TrackNodeComponent>.RegisterTarget.Get,
				handler =>
				{
					if (nodePool[handler] is { } node && nodeSelectDataset.Contains(node))
					{
						ApplySelectedColor(handler, node);
					}
				})
		};

		// Private
		[Inject] private readonly TrackNodeSelectDataset nodeSelectDataset = default!;
		[Inject] [Key("track-decoration")] private readonly IViewPool<ChartComponent> decoratorPool = default!;
		[Inject] private readonly IViewPool<TrackNodeComponent> nodePool = default!;

		// Defined Functions
		private void OnNodeSelectedOrUpdated(TrackNodeComponent node)
		{
			if (nodePool[node] is { } handler)
			{
				ApplySelectedColor(handler, node);
			}

			if (decoratorPool[node.Track] is { } trackHandler)
			{
				trackHandler.Script<EditTrackContent>().MovementContent.IsShow1 = node.IsFirst;
			}
		}

		private static void ApplySelectedColor(PrefabHandler handler, TrackNodeComponent node)
		{
			var content = handler.Script<IEditMoveItemContent>();
			content.BgColorModifier.Register(_ => node.Type switch
			{
				TrackNodeType.Left => ISingleton<TrackLineSetting>.Instance.SelectedLeftColor,
				TrackNodeType.Right => ISingleton<TrackLineSetting>.Instance.SelectedRightColor,
				TrackNodeType.Pos => ISingleton<TrackLineSetting>.Instance.SelectedPosColor,
				TrackNodeType.Width => ISingleton<TrackLineSetting>.Instance.SelectedWidthColor,
				_ => Color.white
			}, 1);
		}
	}
}

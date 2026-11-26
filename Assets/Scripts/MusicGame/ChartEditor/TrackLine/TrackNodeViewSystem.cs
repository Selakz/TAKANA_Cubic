#nullable enable

using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	public class TrackNodeViewSystem : HierarchySystem<TrackNodeViewSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority sideColorPriority = default!;
		[SerializeField] private SequencePriority selectColorPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolRegistrar<TrackNodeComponent>(viewPool,
				ViewPoolRegistrar<TrackNodeComponent>.RegisterTarget.Get,
				UpdateNodeView),
			new DatasetRegistrar<TrackNodeComponent>(viewPool,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataUpdated,
				node =>
				{
					if (viewPool[node] is { } handler) UpdateNodeView(handler);
				}),
			new DatasetRegistrar<TrackNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataAdded,
				node =>
				{
					if (viewPool[node] is { } handler) UpdateNodeView(handler);
				}),
			new DatasetRegistrar<TrackNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataRemoved,
				node =>
				{
					if (viewPool[node] is { } handler)
					{
						var view = handler.Script<INodeView>();
						view.ColorModifier.Unregister(selectColorPriority.Value);
					}
				}),
			new PropertyRegistrar<ChartComponent?>(chartSelectDataset.CurrentSelecting, () =>
			{
				foreach (var node in viewPool)
				{
					var handler = viewPool[node]!;
					UpdateNodeView(handler);
				}
			})
		};

		// Private
		[Inject] private readonly ChartSelectDataset chartSelectDataset = default!;
		[Inject] private readonly TrackNodeSelectDataset nodeSelectDataset = default!;
		[Inject] private readonly IViewPool<TrackNodeComponent> viewPool = default!;

		// Defined Functions
		private void UpdateNodeView(PrefabHandler handler)
		{
			var node = viewPool[handler]!;
			var view = handler.Script<INodeView>();
			if (nodeSelectDataset.Contains(node))
			{
				view.ColorModifier.Register(
					_ => node.Type switch
					{
						TrackNodeType.Left => ISingleton<TrackLineSetting>.Instance.SelectedLeftColor,
						TrackNodeType.Right => ISingleton<TrackLineSetting>.Instance.SelectedRightColor,
						TrackNodeType.Pos => ISingleton<TrackLineSetting>.Instance.SelectedPosColor,
						TrackNodeType.Width => ISingleton<TrackLineSetting>.Instance.SelectedWidthColor,
						_ => Color.white
					},
					selectColorPriority.Value);
			}
			else
			{
				view.ColorModifier.Unregister(selectColorPriority.Value);
			}

			view.ColorModifier.Register(
				_ => node.Type switch
				{
					TrackNodeType.Left => ISingleton<TrackLineSetting>.Instance.LeftSideNodeColor,
					TrackNodeType.Right => ISingleton<TrackLineSetting>.Instance.RightSideNodeColor,
					TrackNodeType.Pos => ISingleton<TrackLineSetting>.Instance.PosSideNodeColor,
					TrackNodeType.Width => ISingleton<TrackLineSetting>.Instance.WidthSideNodeColor,
					_ => Color.white
				},
				sideColorPriority.Value);
			view.IsEditable = ISingleton<TrackLineSetting>.Instance.AllowMultipleEdit ||
			                  node.Track == chartSelectDataset.CurrentSelecting.Value;
		}
	}
}

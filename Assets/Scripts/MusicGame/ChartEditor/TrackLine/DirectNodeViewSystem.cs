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
	public class DirectNodeViewSystem : HierarchySystem<DirectNodeViewSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority sideColorPriority = default!;
		[SerializeField] private SequencePriority selectColorPriority = default!;

		// Private
		private ChartSelectDataset chartSelectDataset = default!;
		private DirectNodeSelectDataset nodeSelectDataset = default!;
		private IViewPool<DirectNodeComponent> viewPool = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolRegistrar<DirectNodeComponent>(viewPool,
				ViewPoolRegistrar<DirectNodeComponent>.RegisterTarget.Get,
				UpdateNodeView),
			new DatasetRegistrar<DirectNodeComponent>(viewPool,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataUpdated,
				component =>
				{
					if (viewPool[component] is { } handler) UpdateNodeView(handler);
				}),
			new DatasetRegistrar<DirectNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataAdded,
				component =>
				{
					if (viewPool[component] is { } handler) UpdateNodeView(handler);
				}),
			new DatasetRegistrar<DirectNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					if (viewPool[component] is { } handler)
					{
						var view = handler.Script<INodeView>();
						view.ColorModifier.Unregister(selectColorPriority.Value);
					}
				}),
			new PropertyRegistrar<ChartComponent?>(chartSelectDataset.CurrentSelecting, () =>
			{
				foreach (var component in viewPool)
				{
					var handler = viewPool[component]!;
					UpdateNodeView(handler);
				}
			})
		};

		// Defined Functions
		[Inject]
		private void Construct(
			ChartSelectDataset chartSelectDataset,
			DirectNodeSelectDataset nodeSelectDataset,
			IViewPool<DirectNodeComponent> viewPool)
		{
			this.chartSelectDataset = chartSelectDataset;
			this.nodeSelectDataset = nodeSelectDataset;
			this.viewPool = viewPool;
		}

		private void UpdateNodeView(PrefabHandler handler)
		{
			var component = viewPool[handler]!;
			var view = handler.Script<INodeView>();
			if (nodeSelectDataset.Contains(component))
			{
				view.ColorModifier.Register(
					_ => component.Locator.IsPos
						? ISingleton<TrackLineSetting>.Instance.SelectedPosColor
						: ISingleton<TrackLineSetting>.Instance.SelectedWidthColor,
					selectColorPriority.Value);
			}
			else view.ColorModifier.Unregister(selectColorPriority.Value);

			view.ColorModifier.Register(
				_ => component.Locator.IsPos
					? ISingleton<TrackLineSetting>.Instance.PosSideNodeColor
					: ISingleton<TrackLineSetting>.Instance.WidthSideNodeColor,
				sideColorPriority.Value);
			view.IsEditable = ISingleton<TrackLineSetting>.Instance.AllowMultipleEdit ||
			                  component.Locator.Track == chartSelectDataset.CurrentSelecting.Value;
		}
	}
}
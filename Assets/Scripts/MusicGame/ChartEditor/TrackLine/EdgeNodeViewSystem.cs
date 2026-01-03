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
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine
{
	public class EdgeNodeViewSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority sideColorPriority = default!;
		[SerializeField] private SequencePriority selectColorPriority = default!;

		// Private
		private ChartSelectDataset chartSelectDataset = default!;
		private EdgeNodeSelectDataset nodeSelectDataset = default!;
		private IViewPool<EdgeNodeComponent> viewPool = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolRegistrar<EdgeNodeComponent>(viewPool,
				ViewPoolRegistrar<EdgeNodeComponent>.RegisterTarget.Get,
				UpdateNodeView),
			new DatasetRegistrar<EdgeNodeComponent>(viewPool,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataUpdated,
				component =>
				{
					if (viewPool[component] is { } handler) UpdateNodeView(handler);
				}),
			new DatasetRegistrar<EdgeNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataAdded,
				component =>
				{
					if (viewPool[component] is { } handler) UpdateNodeView(handler);
				}),
			new DatasetRegistrar<EdgeNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					if (viewPool[component] is { } handler)
					{
						var view = handler.Script<PositionNodeView>();
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
			EdgeNodeSelectDataset nodeSelectDataset,
			IViewPool<EdgeNodeComponent> viewPool)
		{
			this.chartSelectDataset = chartSelectDataset;
			this.nodeSelectDataset = nodeSelectDataset;
			this.viewPool = viewPool;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		private void UpdateNodeView(PrefabHandler handler)
		{
			var component = viewPool[handler]!;
			var view = handler.Script<PositionNodeView>();
			if (nodeSelectDataset.Contains(component))
			{
				view.ColorModifier.Register(
					_ => component.Locator.IsLeft
						? ISingleton<TrackLineSetting>.Instance.SelectedLeftColor
						: ISingleton<TrackLineSetting>.Instance.SelectedRightColor,
					selectColorPriority.Value);
			}
			else view.ColorModifier.Unregister(selectColorPriority.Value);

			view.ColorModifier.Register(
				_ => component.Locator.IsLeft
					? ISingleton<TrackLineSetting>.Instance.LeftSideNodeColor
					: ISingleton<TrackLineSetting>.Instance.RightSideNodeColor,
				sideColorPriority.Value);
			view.IsEditable = ISingleton<TrackLineSetting>.Instance.AllowMultipleEdit ||
			                  component.Locator.Track == chartSelectDataset.CurrentSelecting.Value;
		}
	}
}
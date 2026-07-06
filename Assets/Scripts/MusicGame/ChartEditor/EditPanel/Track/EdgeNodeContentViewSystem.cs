#nullable enable

using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.TrackLine;
using MusicGame.Models.Track;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class EdgeNodeContentViewSystem : HierarchySystem<EdgeNodeContentViewSystem>
	{
		// Serializable and Public
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			// Edge Movement
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataAdded,
				EdgeDataAddedOrUpdated),
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataUpdated,
				EdgeDataAddedOrUpdated),
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					if (edgeNodeViewPool[component] is not { } handler) return;
					var content = handler.Script<IEditMoveItemContent>();
					content.BgColorModifier.Unregister(1);
				}),
			// Direct Movement
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataAdded,
				DirectDataAddedOrUpdated),
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataUpdated,
				DirectDataAddedOrUpdated),
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					if (directNodeViewPool[component] is not { } handler) return;
					var content = handler.Script<IEditMoveItemContent>();
					content.BgColorModifier.Unregister(1);
				})
		};

		// Private
		[Inject] private EdgeNodeSelectDataset edgeDataset = default!;
		[Inject] private IViewPool<EdgeComponent> edgeViewPool = default!;
		[Inject] private IViewPool<EdgeNodeComponent> edgeNodeViewPool = default!;
		[Inject] private DirectNodeSelectDataset directDataset = default!;
		[Inject] private IViewPool<DirectComponent> directViewPool = default!;
		[Inject] private IViewPool<DirectNodeComponent> directNodeViewPool = default!;

		// Defined Functions
		private void EdgeDataAddedOrUpdated(EdgeNodeComponent component)
		{
			if (edgeNodeViewPool[component] is not { } handler) return;
			var content = handler.Script<IEditMoveItemContent>();
			content.BgColorModifier.Register(_ => component.Locator.IsLeft
				? ISingleton<TrackLineSetting>.Instance.SelectedLeftColor
				: ISingleton<TrackLineSetting>.Instance.SelectedRightColor, 1);

			var movement = (component.Locator.Track.Model as ITrack)?.Movement;
			foreach (var edgeComponent in edgeViewPool)
			{
				if (edgeComponent.Model == movement)
				{
					if (edgeViewPool[edgeComponent]?.Script<EditTrackMovementContent>() is { } movementContent)
					{
						movementContent.IsShow1 = component.Locator.IsLeft;
					}
				}
			}
		}

		private void DirectDataAddedOrUpdated(DirectNodeComponent component)
		{
			if (directNodeViewPool[component] is not { } handler) return;
			var content = handler.Script<IEditMoveItemContent>();
			content.BgColorModifier.Register(_ => component.Locator.IsPos
				? ISingleton<TrackLineSetting>.Instance.SelectedPosColor
				: ISingleton<TrackLineSetting>.Instance.SelectedWidthColor, 1);

			var movement = (component.Locator.Track.Model as ITrack)?.Movement;
			foreach (var directComponent in directViewPool)
			{
				if (directComponent.Model == movement)
				{
					if (directViewPool[directComponent]?.Script<EditTrackMovementContent>() is { } movementContent)
					{
						movementContent.IsShow1 = component.Locator.IsPos;
					}
				}
			}
		}
	}
}
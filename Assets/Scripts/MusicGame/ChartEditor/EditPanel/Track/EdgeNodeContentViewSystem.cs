#nullable enable

using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.TrackLine;
using MusicGame.Models.Track;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class EdgeNodeContentViewSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			// Edge Movement
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataAdded,
				component =>
				{
					if (edgeNodeViewPool[component] is not { } handler) return;
					var content = handler.Script<IEditMoveItemContent>();
					content.BgColorModifier.Register(
						_ => component.Locator.IsLeft
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
				}),
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
				component =>
				{
					if (directNodeViewPool[component] is not { } handler) return;
					var content = handler.Script<IEditMoveItemContent>();
					content.BgColorModifier.Register(
						_ => component.Locator.IsPos
							? ISingleton<TrackLineSetting>.Instance.SelectedPosColor
							: ISingleton<TrackLineSetting>.Instance.SelectedWidthColor, 1);

					var movement = (component.Locator.Track.Model as ITrack)?.Movement;
					foreach (var directComponent in directViewPool)
					{
						if (directComponent.Model == movement)
						{
							if (directViewPool[directComponent]?.Script<EditTrackMovementContent>() is
							    { } movementContent)
							{
								movementContent.IsShow1 = component.Locator.IsPos;
							}
						}
					}
				}),
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
		private EdgeNodeSelectDataset edgeDataset = default!;
		private IViewPool<EdgeComponent> edgeViewPool = default!;
		private IViewPool<EdgeNodeComponent> edgeNodeViewPool = default!;
		private DirectNodeSelectDataset directDataset = default!;
		private IViewPool<DirectComponent> directViewPool = default!;
		private IViewPool<DirectNodeComponent> directNodeViewPool = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			IViewPool<EdgeComponent> edgeViewPool,
			IViewPool<EdgeNodeComponent> edgeNodeViewPool,
			EdgeNodeSelectDataset edgeDataset,
			IViewPool<DirectComponent> directViewPool,
			IViewPool<DirectNodeComponent> directNodeViewPool,
			DirectNodeSelectDataset directDataset)
		{
			this.edgeViewPool = edgeViewPool;
			this.edgeNodeViewPool = edgeNodeViewPool;
			this.edgeDataset = edgeDataset;
			this.directViewPool = directViewPool;
			this.directNodeViewPool = directNodeViewPool;
			this.directDataset = directDataset;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}
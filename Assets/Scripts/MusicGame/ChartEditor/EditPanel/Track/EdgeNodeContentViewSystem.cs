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
			new DatasetRegistrar<EdgeNodeComponent>(dataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataAdded,
				component =>
				{
					if (nodeViewPool[component] is not { } handler) return;
					var content = handler.Script<EditV1EItemContent>();
					content.BgColorModifier.Register(
						_ => component.Locator.IsLeft
							? ISingleton<TrackLineSetting>.Instance.SelectedLeftColor
							: ISingleton<TrackLineSetting>.Instance.SelectedRightColor, 1);

					var movement = (component.Locator.Track.Model as ITrack)?.Movement;
					foreach (var edgeComponent in edgeViewPool)
					{
						if (edgeComponent.Model == movement)
						{
							if (edgeViewPool[edgeComponent]?.Script<EditEdgeMovementContent>() is { } movementContent)
							{
								movementContent.IsShowLeft = component.Locator.IsLeft;
							}
						}
					}
				}),
			new DatasetRegistrar<EdgeNodeComponent>(dataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					if (nodeViewPool[component] is not { } handler) return;
					var content = handler.Script<EditV1EItemContent>();
					content.BgColorModifier.Unregister(1);
				})
		};

		// Private
		private EdgeNodeSelectDataset dataset = default!;
		private IViewPool<EdgeComponent> edgeViewPool = default!;
		private IViewPool<EdgeNodeComponent> nodeViewPool = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			IViewPool<EdgeComponent> edgeViewPool,
			IViewPool<EdgeNodeComponent> nodeViewPool,
			EdgeNodeSelectDataset dataset)
		{
			this.edgeViewPool = edgeViewPool;
			this.nodeViewPool = nodeViewPool;
			this.dataset = dataset;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}
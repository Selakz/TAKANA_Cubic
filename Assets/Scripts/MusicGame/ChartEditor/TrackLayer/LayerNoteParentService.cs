#nullable enable

using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Track;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class LayerNoteParentService : HierarchySystem<LayerNoteParentService>, INoteParentService
	{
		// Serializable and Public
		public override bool AsImplementedInterfaces => true;

		// Private
		private ChartSelectDataset selectDataset = default!;
		private StageMouseTimeRetriever timeRetriever = default!;
		private StageMouseWidthRetriever widthRetriever = default!;

		private IReadOnlyDataset<ChartComponent> viewPoolSubSet = default!;

		// Defined Functions
		public ChartComponent? GetParent(SingleNotePlaceType notePlaceType)
		{
			switch (notePlaceType)
			{
				case SingleNotePlaceType.SelectedTrack:
					var parent = selectDataset.CurrentSelecting.Value;
					return parent?.Model is ITrack ? parent : null;
				case SingleNotePlaceType.NearestTrack:
					if (!timeRetriever.GetMouseTimeStart(out var time) ||
					    !widthRetriever.GetMouseGamePoint(out var point)) return null;

					ChartComponent? target = null;
					var mousePosition = point.x;
					var minDistance = float.MaxValue;
					foreach (var component in viewPoolSubSet)
					{
						if (component.Model is not ITrack model ||
						    component.GetLayerInfo() is not { IsDecoration: false, IsSelected: true }) continue;
						var trackPosition = model.Movement.GetPos(time);
						var distance = Mathf.Abs(trackPosition - mousePosition);
						if (distance < minDistance)
						{
							minDistance = distance;
							target = component;
						}
					}

					return target;
				default:
					return null;
			}
		}

		// Constructor
		[Inject]
		private void Construct(
			ChartSelectDataset selectDataset,
			StageMouseTimeRetriever timeRetriever,
			StageMouseWidthRetriever widthRetriever,
			[Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.selectDataset = selectDataset;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;

			viewPoolSubSet = viewPool.SubDataset(T3ChartClassifier.Instance, T3Flag.Track);
		}
	}
}
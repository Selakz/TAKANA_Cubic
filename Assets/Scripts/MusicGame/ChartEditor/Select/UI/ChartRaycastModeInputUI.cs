#nullable enable

using System.Collections.Generic;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.Gameplay.Basic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Preset.Select;
using T3Framework.Preset.Select.UI;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Select.UI
{
	public class ChartRaycastModeInputUI : RaycastModeInputUI<ChartComponent>, ISelfInstaller
	{
		// Serializable and Public
		protected override IComparer<KeyValuePair<RaycastHit, ChartComponent>>? PollingComparer => pollingComparer ??=
			Comparer<KeyValuePair<RaycastHit, ChartComponent>>.Create((x, y) =>
			{
				if (x.Value.Model is ITrack model1 && y.Value.Model is ITrack model2 &&
				    timeRetriever.GetMouseTimeStart(out var time))
				{
					var localPoint1 = x.Key.collider.transform.InverseTransformPoint(x.Key.point);
					var localPoint2 = y.Key.collider.transform.InverseTransformPoint(y.Key.point);
					var distance1 = Mathf.Min(
						Mathf.Abs(localPoint1.x),
						Mathf.Abs(localPoint1.x - model1.Movement.GetWidth(time) / 2));
					var distance2 = Mathf.Min(
						Mathf.Abs(localPoint2.x),
						Mathf.Abs(localPoint2.x - model2.Movement.GetWidth(time) / 2));

					return distance1.CompareTo(distance2);
				}

				if (x.Value.Model is INote && y.Value.Model is INote &&
				    x.Key.transform.parent.TryGetComponent<IT3ModelViewPresenter>(out var presenter1) &&
				    y.Key.transform.parent.TryGetComponent<IT3ModelViewPresenter>(out var presenter2))
				{
					return presenter2.MainTexture.SortingOrderModifier.Value.CompareTo(
						presenter1.MainTexture.SortingOrderModifier.Value);
				}

				return x.Key.colliderInstanceID.CompareTo(y.Key.colliderInstanceID);
			});

		protected override NotifiableProperty<ISelectRaycastMode<ChartComponent>> RaycastMode => raycastMode;

		// Private
		private NotifiableProperty<ISelectRaycastMode<ChartComponent>> raycastMode = default!;

		private IComparer<KeyValuePair<RaycastHit, ChartComponent>>? pollingComparer;
		private StageMouseTimeRetriever timeRetriever = default!;

		// Defined Functions
		[Inject]
		protected void Construct(
			NotifiableProperty<ISelectRaycastMode<ChartComponent>> raycastMode,
			StageMouseTimeRetriever timeRetriever)
		{
			this.raycastMode = raycastMode;
			this.timeRetriever = timeRetriever;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}
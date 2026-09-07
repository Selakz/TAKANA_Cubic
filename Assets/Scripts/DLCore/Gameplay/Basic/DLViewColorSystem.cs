#nullable enable

using System.Collections.Generic;
using DLCore.Models;
using DLCore.Models.Note;
using DLCore.Models.Track;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Basic;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace DLCore.Gameplay.Basic
{
	public class DLViewColorSystem : HierarchySystem<DLViewColorSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority colorPriority = default!;
		[SerializeField] private InspectorDictionary<DLFlag, InspectorDictionary<ColorVariant, Color>> noteColors = new();
		[SerializeField] private InspectorDictionary<ColorVariant, Color> trackColors = new();

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					var component = viewPool[handler]!;
					if (component.Model is IDLNote note)
					{
						var presenter = handler.Script<IT3ModelViewPresenter>();
						foreach (var (flag, colorMap) in noteColors.Value)
						{
							if (!DLChartClassifier.Instance.IsOfType(component, flag)) continue;
							foreach (var modifier in presenter.ColorModifiers)
							{
								modifier.Register(value => colorMap.Value.GetValueOrDefault(note.Color, value),
									colorPriority);
							}

							break;
						}
					}
					else if (component.Model is IDLTrack track)
					{
						var presenter = handler.Script<IT3ModelViewPresenter>();
						foreach (var modifier in presenter.ColorModifiers)
						{
							modifier.Register(value =>
								{
									var gradient = track.ColorMovement.GetPos(music.ChartTime);
									var from = trackColors.Value.GetValueOrDefault(gradient.FromColor, value);
									var to = trackColors.Value.GetValueOrDefault(gradient.ToColor, value);
									return Color.Lerp(from, to, gradient.Gradient);
								},
								colorPriority);
						}
					}
				},
				() =>
				{
					var component = viewPool[handler]!;
					if (component.Model is not IDLNote && component.Model is not IDLTrack) return;
					var presenter = handler.Script<IT3ModelViewPresenter>();
					foreach (var modifier in presenter.ColorModifiers)
					{
						modifier.Unregister(colorPriority, true);
					}
				}))
		};

		// Private
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
		[Inject] private IGameAudioPlayer music = default!;
	}
}
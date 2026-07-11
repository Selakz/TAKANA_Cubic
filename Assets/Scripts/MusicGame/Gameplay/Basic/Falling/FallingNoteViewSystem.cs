#nullable enable

using System;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Gameplay.Stage;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Basic.Falling
{
	public class FallingNoteViewSystem : HierarchySystem<FallingNoteViewSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority positionPriority = default!;
		[SerializeField] private SequencePriority widthPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<GameplayStageSkinConfig>(service.OnStageReset,
				config => IsEnabled = config.trackBehaviour is TrackBehaviour.Falling)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					var component = viewPool[handler]!;
					if (!T3ChartClassifier.Instance.IsOfType(component, T3Flag.Live | T3Flag.Note) ||
					    component.Model is not INote note) return;
					var presenter = handler.Script<T3NoteViewPresenter>();
					presenter.PositionModifier.Register(
						value =>
						{
							var x = component.Parent?.Model is ITrack track
								? value.x + track.Movement.GetPos(Mathf.Max(note.TimeJudge, music.ChartTime))
								: value.x;
							var y = note.Movement.GetPos(music.ChartTime);
							return new(x, y);
						},
						positionPriority);
					Func<Vector2, Vector2> function = value =>
					{
						if (component.Parent?.Model is not ITrack track) return new(1, value.y);
						var width = track.Movement.GetWidth(Mathf.Max(note.TimeJudge, music.ChartTime));
						var baseGap = ISingleton<PlayfieldSetting>.Instance.TrackGap.Value;
						var gap = note is not Hit { Type: HitType.Slide }
							? baseGap
							: Mathf.Max(2, 8 * width / StageWidth) * baseGap;
						return new(
							width > 2 * gap ? width - gap : width,
							value.y);
					};
					foreach (var modifier in presenter.WidthModifiers)
					{
						modifier.Register(function, widthPriority);
					}
				},
				() =>
				{
					var note = viewPool[handler]!;
					if (!T3ChartClassifier.Instance.IsOfType(note, T3Flag.Live | T3Flag.Note)) return;
					var presenter = handler.Script<T3NoteViewPresenter>();
					presenter.PositionModifier.Unregister(positionPriority, true);
					foreach (var modifier in presenter.WidthModifiers)
					{
						modifier.Unregister(widthPriority, true);
					}
				}))
		};

		// Private
		[Inject] private IGameAudioPlayer music = default!;
		[Inject] private IStageViewGenerateService service = default!;
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;

		// Static
		private const float StageWidth = 9f;
	}
}
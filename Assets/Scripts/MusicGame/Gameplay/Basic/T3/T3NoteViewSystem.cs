#nullable enable

using System;
using MusicGame.Gameplay.Audio;
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

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3NoteViewSystem : HierarchySystem<T3NoteViewSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority positionPriority = default!;
		[SerializeField] private SequencePriority widthPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<GameplayStageSkinConfig>(service.OnStageReset,
				config => IsEnabled = config.trackBehaviour is TrackBehaviour.Instant)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					var note = viewPool[handler]!;
					if (!T3ChartClassifier.Instance.IsOfType(note, T3Flag.Live | T3Flag.Note)) return;
					var presenter = handler.Script<T3NoteViewPresenter>();
					presenter.PositionModifier.Register(
						value => new(value.x, (note.Model as INote)!.Movement.GetPos(music.ChartTime)),
						positionPriority);
					Func<Vector2, Vector2> function = value =>
					{
						if (note.Parent?.Model is not ITrack track) return new(1, value.y);
						var width = track.Movement.GetWidth(music.ChartTime);
						var baseGap = ISingleton<PlayfieldSetting>.Instance.TrackGap.Value;
						var gap = note.Model is not Hit { Type: HitType.Slide }
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
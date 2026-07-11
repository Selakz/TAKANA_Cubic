#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	public class AutoScoreViewSystem : HierarchySystem<AutoScoreViewSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority baseColorPriority = default!;
		[SerializeField] private SequencePriority startColorPriority = default!;
		[SerializeField] private SequencePriority positionPriority = default!;
		[SerializeField] private SequencePriority heightPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() => OnDataAdded(viewPool[handler]!),
				() => BeforeDataRemoved(viewPool[handler]!)))
		};

		// Private
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
		[Inject] private IGameAudioPlayer music = default!;

		// Event Handlers
		private void OnDataAdded(ChartComponent note)
		{
			if (note.Model is not INote model) return;

			var presenter = viewPool[note]!.Script<T3NoteViewPresenter>();
			foreach (var cm in presenter.ColorModifiers)
			{
				cm.Register(color => music.ChartTime < model.TimeMax
					? model.IsDummy()
						? color with { a = color.a * ISingleton<AutoScoreSetting>.Instance.DummyNoteOpacity }
						: color
					: Color.clear, baseColorPriority, true);
			}

			presenter.MainTexture.ColorModifier.Register(color =>
				music.ChartTime < model.TimeJudge ? color : Color.clear, startColorPriority, true);

			if (model is Hold hold)
			{
				foreach (var modifier in presenter.HeightModifiers)
				{
					modifier.Register(value =>
					{
						if (music.ChartTime < hold.TimeJudge) return value;
						var y = Mathf.Max(0, hold.TailMovement.GetPos(music.ChartTime));
						return new(value.x, y);
					}, heightPriority, true);
				}

				presenter.PositionModifier.Register(
					pos => music.ChartTime < hold.TimeJudge ? pos : new(pos.x, 0),
					positionPriority, true);
			}
		}

		private void BeforeDataRemoved(ChartComponent note)
		{
			if (note.Model is not INote model) return;

			var presenter = viewPool[note]!.Script<T3NoteViewPresenter>();
			foreach (var cm in presenter.ColorModifiers) cm.Unregister(baseColorPriority, true);
			presenter.MainTexture.ColorModifier.Unregister(startColorPriority, true);

			if (model is Hold)
			{
				foreach (var modifier in presenter.HeightModifiers) modifier.Unregister(heightPriority, true);
				presenter.PositionModifier.Unregister(positionPriority, true);
			}
		}

		// System Functions
		protected override void OnDestroy()
		{
			base.OnDestroy();
			viewPool.Dispose();
		}
	}
}
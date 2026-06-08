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
		[SerializeField] private SequencePriority colorPriority = default!;
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
			presenter.MainTexture.ColorModifier.Register(color => music.ChartTime < model.TimeJudge
				? model.IsDummy()
					? color with { a = color.a * ISingleton<AutoScoreSetting>.Instance.DummyNoteOpacity }
					: color
				: Color.clear, colorPriority, true);

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

				presenter.Textures["body"].ColorModifier.Register(color => music.ChartTime < hold.TimeEnd
					? model.IsDummy()
						? color with { a = color.a * ISingleton<AutoScoreSetting>.Instance.DummyNoteOpacity }
						: color
					: Color.clear, colorPriority, true);
			}
		}

		private void BeforeDataRemoved(ChartComponent note)
		{
			if (note.Model is not INote model) return;

			var presenter = viewPool[note]!.Script<T3NoteViewPresenter>();
			presenter.MainTexture.ColorModifier.Unregister(colorPriority, true);

			if (model is Hold)
			{
				foreach (var modifier in presenter.HeightModifiers) modifier.Unregister(heightPriority, true);
				presenter.PositionModifier.Unregister(positionPriority, true);
				presenter.Textures["body"].ColorModifier.Unregister(colorPriority, true);
			}
		}

		// System Functions
		void Update()
		{
			foreach (var note in viewPool)
			{
				if (note.Model is not INote) return;
				var presenter = viewPool[note]!.Script<T3NoteViewPresenter>();
				presenter.MainTexture.ColorModifier.Update();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			viewPool.Dispose();
		}
	}
}
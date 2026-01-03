#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Runtime.ECS;
using T3Framework.Static;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	public class AutoScoreViewSystem : T3System, ITickable
	{
		private readonly GameAudioPlayer music;
		private readonly int colorPriority;
		private readonly int positionPriority;
		private readonly int heightPriority;

		public IViewPool<ChartComponent> NotePool { get; }

		public AutoScoreViewSystem(
			[Key("stage")] IViewPool<ChartComponent> viewPool,
			GameAudioPlayer music,
			int colorPriority,
			int positionPriority,
			int heightPriority) : base(true)
		{
			NotePool = new SubViewPool<ChartComponent, T3Flag>
				(viewPool, new T3ChartClassifier(), T3Flag.Note);
			this.music = music;
			this.colorPriority = colorPriority;
			this.positionPriority = positionPriority;
			this.heightPriority = heightPriority;
			NotePool.OnDataAdded += OnDataAdded;
			NotePool.BeforeDataRemoved += BeforeDataRemoved;
		}

		public void Tick()
		{
			foreach (var note in NotePool)
			{
				var presenter = NotePool[note]!.Script<T3NoteViewPresenter>();
				presenter.Textures["main"].ColorModifier.Update();
			}
		}

		private void OnDataAdded(ChartComponent note)
		{
			if (note.Model is not INote model) return;

			var presenter = NotePool[note]!.Script<T3NoteViewPresenter>();
			// TODO: IModifier, UnionModifier, ColorModifier in IT3ModelViewPresenter
			presenter.Textures["main"].ColorModifier.Register(color => music.ChartTime < model.TimeJudge
				? model.IsDummy()
					? new Color(color.r, color.g, color.b,
						color.a * ISingleton<AutoScoreSetting>.Instance.DummyNoteOpacity)
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
			}
		}

		private void BeforeDataRemoved(ChartComponent note)
		{
			if (note.Model is not INote) return;

			var presenter = NotePool[note]!.Script<T3NoteViewPresenter>();
			presenter.Textures["main"].ColorModifier.Unregister(colorPriority, true);
		}

		public override void Dispose()
		{
			base.Dispose();
			NotePool.OnDataAdded -= OnDataAdded;
			NotePool.BeforeDataRemoved -= BeforeDataRemoved;
			NotePool.Dispose();
		}
	}
}
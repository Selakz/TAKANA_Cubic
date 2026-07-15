#nullable enable

using System;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using MusicGame.Gameplay.Level;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.JudgeScore
{
	public class JudgeScoreViewSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority startJudgeColorPriority = default!;
		[SerializeField] private SequencePriority missHoldColorPriority = default!;
		[SerializeField] private SequencePriority positionPriority = default!;
		[SerializeField] private SequencePriority heightPriority = default!;

		public IViewPool<ChartComponent> NotePool { get; private set; } = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action<IJudgeItem>>(
				e => judgeStorage.OnJudgeItemAdded += e,
				e => judgeStorage.OnJudgeItemAdded += e,
				OnJudgeItemAdded),
			new DatasetRegistrar<ChartComponent>(NotePool,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataRemoved,
				BeforeDataRemoved)
		};

		// Private
		private IGameAudioPlayer music = default!;
		private JudgeStorage judgeStorage = default!;

		// Constructor
		[Inject]
		private void Construct(
			[Key("stage")] IViewPool<ChartComponent> viewPool,
			IGameAudioPlayer music,
			JudgeStorage judgeStorage)
		{
			NotePool = new SubViewPool<ChartComponent, T3Flag>(
				viewPool, new T3ChartClassifier(), T3Flag.Note);
			this.music = music;
			this.judgeStorage = judgeStorage;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void OnJudgeItemAdded(IJudgeItem judgeItem)
		{
			if (judgeItem.ComboItem.FromComponent is not { Model: INote model } note ||
			    NotePool[note] is not { } handler) return;

			var presenter = handler.Script<T3NoteViewPresenter>();
			presenter.MainTexture.ColorModifier.Assign(Color.clear, startJudgeColorPriority);

			// TODO: This should be done at hold generated
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

				var a = judgeItem is IT3JudgeItem { JudgeResult: T3JudgeResult.EarlyMiss or T3JudgeResult.LateMiss }
					? ISingleton<PlayfieldSetting>.Instance.MissHoldOpacity
					: 1f;
				foreach (var cm in presenter.ColorModifiers)
				{
					cm.Register(color =>
						{
							if (Mathf.Approximately(color.a, 0f)) return color;
							if (music.ChartTime >= hold.TimeEnd) return Color.clear;
							return color with { a = a };
						},
						missHoldColorPriority, true);
				}
			}
		}

		private void BeforeDataRemoved(ChartComponent note)
		{
			if (note.Model is not INote model) return;

			var presenter = NotePool[note]!.Script<T3NoteViewPresenter>();
			presenter.MainTexture.ColorModifier.Unregister(startJudgeColorPriority);

			if (model is Hold)
			{
				foreach (var modifier in presenter.HeightModifiers)
				{
					modifier.Unregister(heightPriority, true);
				}

				presenter.PositionModifier.Unregister(positionPriority, true);
				foreach (var cm in presenter.ColorModifiers) cm.Unregister(missHoldColorPriority, true);
			}
		}

		// System Functions
		protected override void OnDestroy()
		{
			base.OnDestroy();
			NotePool.Dispose();
		}
	}
}
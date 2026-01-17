#nullable enable

using System;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Runtime.ECS;
using T3Framework.Static;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3NoteViewSystem : IInitializable
	{
		private readonly int positionPriority;
		private readonly int widthPriority;
		private readonly IGameAudioPlayer music;
		private readonly IViewPool<ChartComponent> viewPool;
		private readonly SubDataset<ChartComponent, Type> notePool;

		public T3NoteViewSystem(
			int positionPriority,
			int widthPriority,
			IGameAudioPlayer music,
			[Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.positionPriority = positionPriority;
			this.widthPriority = widthPriority;
			this.music = music;
			this.viewPool = viewPool;
			notePool = new SubDataset<ChartComponent, Type>(viewPool, new TypeClassifier<IChartModel>(),
				typeof(Hit), typeof(Hold));
			notePool.OnDataAdded += OnDataAdded;
			notePool.BeforeDataRemoved += BeforeDataRemoved;
		}

		private void OnDataAdded(ChartComponent note)
		{
			var presenter = viewPool[note]!.Script<T3NoteViewPresenter>();
			presenter.PositionModifier.Register(
				value => new(value.x, (note.Model as INote)!.Movement.GetPos(music.ChartTime)),
				positionPriority, true);
			Func<Vector2, Vector2> function = value =>
			{
				if (note.Parent?.Model is not ITrack track) return new(1, value.y);
				var gap = note.Model is not Hit { Type: HitType.Slide }
					? ISingleton<PlayfieldSetting>.Instance.TrackGap1.Value
					: ISingleton<PlayfieldSetting>.Instance.TrackGap2.Value;
				var width = track.Movement.GetWidth(music.ChartTime);
				return new(
					width > 2 * gap ? width - gap : width,
					value.y);
			};
			foreach (var modifier in presenter.WidthModifiers)
			{
				modifier.Register(function, widthPriority, true);
			}
		}

		private void BeforeDataRemoved(ChartComponent note)
		{
			var presenter = viewPool[note]!.Script<T3NoteViewPresenter>();
			presenter.PositionModifier.Unregister(positionPriority, true);
			foreach (var modifier in presenter.WidthModifiers)
			{
				modifier.Unregister(widthPriority, true);
			}
		}

		public void Initialize()
		{
		}
	}
}
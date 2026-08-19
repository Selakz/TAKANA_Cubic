#nullable enable

using System;
using EditorPlugin.PluginSystem;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public abstract class RawTrackMovement
	{
		protected readonly ChartComponent track;
		protected readonly ITrackMovement Movement;
		protected readonly ChartPosMoveList Movement1;
		protected readonly ChartPosMoveList Movement2;

		public abstract string type { get; }

		protected RawTrackMovement(ChartComponent component, StagingRegistry? registry)
		{
			track = component;
			Movement = ((ITrack)track.Model).Movement;
			Movement1 = Movement.Movement1 as ChartPosMoveList
			            ?? throw new InvalidOperationException(
				            $"Unsupported track movement list: {Movement.Movement1.GetType()}");
			Movement2 = Movement.Movement2 as ChartPosMoveList
			            ?? throw new InvalidOperationException(
				            $"Unsupported track movement list: {Movement.Movement2.GetType()}");
		}

		public float getPosition(int timeMilli) => Movement.GetPos(new T3Time(timeMilli));

		public float getWidth(int timeMilli) => Movement.GetWidth(new T3Time(timeMilli));

		public float getLeftPosition(int timeMilli) => Movement.GetLeftPos(new T3Time(timeMilli));

		public float getRightPosition(int timeMilli) => Movement.GetRightPos(new T3Time(timeMilli));
	}
}
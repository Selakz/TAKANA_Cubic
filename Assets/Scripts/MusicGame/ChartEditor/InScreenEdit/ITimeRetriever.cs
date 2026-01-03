using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	/// <summary> If any method returns <see cref="T3Time.MaxValue"/>, it means the time is the end of the music;
	/// If any method returns <see cref="T3Time.MinValue"/>, it means the time is not valid. </summary>
	public interface ITimeRetriever
	{
		public T3Time GetTimeStart(Vector3 position);

		public T3Time GetHoldTimeEnd(Vector3 position);

		public T3Time GetTrackTimeEnd(Vector3 position);
	}

	public class StageMouseTimeRetriever
	{
		private static readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);

		public Camera LevelCamera { get; }

		public NotifiableProperty<ITimeRetriever> TimeRetriever { get; }

		public StageMouseTimeRetriever(
			NotifiableProperty<ITimeRetriever> timeRetriever,
			[Key("stage")] Camera camera)
		{
			TimeRetriever = timeRetriever;
			LevelCamera = camera;
		}

		public bool GetMouseGamePoint(out Vector3 gamePoint)
		{
			var mousePoint = Input.mousePosition;
			if (!LevelCamera.ContainsScreenPoint(mousePoint) ||
			    !LevelCamera.ScreenToWorldPoint(gamePlane, mousePoint, out gamePoint))
			{
				gamePoint = Vector3.zero;
				return false;
			}

			return true;
		}

		public bool GetMouseTimeStart(out T3Time time)
		{
			if (!GetMouseGamePoint(out var gamePoint))
			{
				time = T3Time.MinValue;
				return false;
			}

			time = TimeRetriever.Value.GetTimeStart(gamePoint);
			return true;
		}

		public bool GetMouseHoldTimeEnd(out T3Time time)
		{
			if (!GetMouseGamePoint(out var gamePoint))
			{
				time = T3Time.MinValue;
				return false;
			}

			time = TimeRetriever.Value.GetHoldTimeEnd(gamePoint);
			return true;
		}

		public bool GetMouseTrackTimeEnd(out T3Time time)
		{
			if (!GetMouseGamePoint(out var gamePoint))
			{
				time = T3Time.MinValue;
				return false;
			}

			time = TimeRetriever.Value.GetTrackTimeEnd(gamePoint);
			return true;
		}
	}
}
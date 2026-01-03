using T3Framework.Runtime.Extensions;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public interface IWidthRetriever
	{
		public float GetWidth(Vector3 position);

		public float GetPosition(Vector3 position);

		public float GetAttachedPosition(Vector3 position);
	}

	public class StageMouseWidthRetriever
	{
		private static readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);

		public Camera LevelCamera { get; }

		public NotifiableProperty<IWidthRetriever> WidthRetriever { get; }

		public StageMouseWidthRetriever(
			NotifiableProperty<IWidthRetriever> widthRetriever,
			[Key("stage")] Camera camera)
		{
			WidthRetriever = widthRetriever;
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

		public bool GetMouseWidth(out float width)
		{
			if (!GetMouseGamePoint(out var gamePoint))
			{
				width = 1;
				return false;
			}

			width = WidthRetriever.Value.GetWidth(gamePoint);
			return true;
		}

		public bool GetMousePosition(out float pos)
		{
			if (!GetMouseGamePoint(out var gamePoint))
			{
				pos = 0;
				return false;
			}

			pos = WidthRetriever.Value.GetPosition(gamePoint);
			return true;
		}

		public bool GetMouseAttachedPosition(out float pos)
		{
			if (!GetMouseGamePoint(out var gamePoint))
			{
				pos = 0;
				return false;
			}

			pos = WidthRetriever.Value.GetAttachedPosition(gamePoint);
			return true;
		}
	}
}
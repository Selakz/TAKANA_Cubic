#nullable enable

using UnityEngine;
using T3Framework.Runtime.Extensions;
using VContainer;

namespace MusicGame.Gameplay.Judge.T3
{
	public class StagePositionRetriever
	{
		private readonly Camera levelCamera;
		private readonly Plane gamePlane;

		public StagePositionRetriever(
			[Key("stage")] Camera levelCamera,
			Plane gamePlane)
		{
			this.levelCamera = levelCamera;
			this.gamePlane = gamePlane;
		}

		public float GetPosition(Vector2 screenPosition)
		{
			return levelCamera.ScreenToWorldPoint(gamePlane, screenPosition, out var gamePoint)
				? gamePoint.x
				: float.MaxValue;
		}
	}
}
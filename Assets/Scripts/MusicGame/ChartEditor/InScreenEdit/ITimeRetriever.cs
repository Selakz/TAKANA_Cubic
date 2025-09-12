using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public interface ITimeRetriever
	{
		public T3Time GetTimeStart(Vector3 position);

		public T3Time GetHoldTimeEnd(Vector3 position);

		public T3Time GetTrackTimeEnd(Vector3 position);

		protected static T3Time GetCorrespondingTime(float y) =>
			y / LevelManager.Instance.LevelSpeed.SpeedRate + LevelManager.Instance.Music.ChartTime;
	}
}
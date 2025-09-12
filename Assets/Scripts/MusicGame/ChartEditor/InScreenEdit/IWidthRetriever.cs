using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public interface IWidthRetriever
	{
		public float GetWidth(Vector3 position);

		public float GetPosition(Vector3 position);

		public float GetAttachedPosition(Vector3 position);
	}
}
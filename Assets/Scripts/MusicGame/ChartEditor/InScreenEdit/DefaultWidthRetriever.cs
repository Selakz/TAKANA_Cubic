using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class DefaultWidthRetriever : IWidthRetriever
	{
		public float GetWidth(Vector3 position)
		{
			return 2f;
		}

		public float GetPosition(Vector3 position)
		{
			return position.x;
		}

		public float GetAttachedPosition(Vector3 position)
		{
			return position.x;
		}
	}
}
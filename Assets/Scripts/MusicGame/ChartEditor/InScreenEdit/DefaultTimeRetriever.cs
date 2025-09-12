using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class DefaultTimeRetriever : ITimeRetriever
	{
		public T3Time GetTimeStart(Vector3 position)
		{
			return ITimeRetriever.GetCorrespondingTime(position.y);
		}

		public T3Time GetHoldTimeEnd(Vector3 position)
		{
			return ITimeRetriever.GetCorrespondingTime(position.y) + 1000;
		}

		public T3Time GetTrackTimeEnd(Vector3 position)
		{
			// TODO: AudioLength to ChartLength
			return ISingletonSetting<InScreenEditSetting>.Instance.IsInitialTrackLengthToEnd
				? LevelManager.Instance.Music.AudioLength
				: ITimeRetriever.GetCorrespondingTime(position.y) +
				  ISingletonSetting<InScreenEditSetting>.Instance.InitialTrackLength;
		}
	}
}
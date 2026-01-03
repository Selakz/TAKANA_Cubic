#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Speed;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public abstract class BaseTimeRetriever : ITimeRetriever
	{
		[Inject] private GameAudioPlayer music = default!;
		[Inject] private NotifiableProperty<ISpeed> speed = default!;

		public virtual T3Time GetTimeStart(Vector3 position) => GetCorrespondingTime(position.y);

		public virtual T3Time GetHoldTimeEnd(Vector3 position) => GetCorrespondingTime(position.y) + 1000;

		public virtual T3Time GetTrackTimeEnd(Vector3 position)
		{
			return ISingletonSetting<InScreenEditSetting>.Instance.IsInitialTrackLengthToEnd
				? music.AudioToChart(music.AudioLength)
				: GetCorrespondingTime(position.y) +
				  ISingletonSetting<InScreenEditSetting>.Instance.InitialTrackLength;
		}

		protected T3Time GetCorrespondingTime(float y) => y / speed.Value.SpeedRate + music.ChartTime;
	}
}
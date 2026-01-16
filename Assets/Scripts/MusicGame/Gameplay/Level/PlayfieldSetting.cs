using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	[Description("Header")]
	public class PlayfieldSetting : ISingletonSetting<PlayfieldSetting>
	{
		[Description("HitSoundVolumePercent")]
		[MinValue(0)]
		[MaxValue(100)]
		public NotifiableProperty<int> HitSoundVolumePercent { get; set; } = new(50);

		[Description("TrackFaceDefaultColor")]
		public NotifiableProperty<Color> TrackFaceDefaultColor { get; set; } = new(Color.black);

		[Description("MissHoldOpacity")]
		[MinValue(0)]
		[MaxValue(1)]
		public NotifiableProperty<float> MissHoldOpacity { get; set; } = new(0.25f);

		[Description("Speed")]
		public NotifiableProperty<float> Speed { get; set; } = new(1);

		[Description("AudioDeviation")]
		public NotifiableProperty<T3Time> AudioDeviation { get; set; } = new(0);

		[Description("TrackGap1")]
		public NotifiableProperty<float> TrackGap1 { get; set; } = new(0.15f);

		[Description("TrackGap2")]
		public NotifiableProperty<float> TrackGap2 { get; set; } = new(0.3f);

		[Description("CameraPosition")]
		public NotifiableProperty<Vector3> CameraPosition { get; set; } = new(Vector3.zero);

		[Description("CameraRotation")]
		public NotifiableProperty<Vector3> CameraRotation { get; set; } = new(Vector3.zero);

		[Description("TimeAfterEnd")]
		[HideInGame]
		public NotifiableProperty<int> TimeAfterEnd { get; set; } = new(2000);

		[Description("UpperThreshold")]
		[HideInGame]
		public NotifiableProperty<float> UpperThreshold { get; set; } = new(15);

		[Description("LowerThreshold")]
		[HideInGame]
		public NotifiableProperty<float> LowerThreshold { get; set; } = new(-5);
	}
}
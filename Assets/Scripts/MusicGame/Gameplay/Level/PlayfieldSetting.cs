using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	[Description("Header")]
	public class PlayfieldSetting : ISingletonSetting<PlayfieldSetting>
	{
		[Description("TrackGap1")]
		public NotifiableProperty<float> TrackGap1 { get; set; } = new(0.15f);

		[Description("TrackGap2")]
		public NotifiableProperty<float> TrackGap2 { get; set; } = new(0.3f);

		[Description("TrackFaceDefaultColor")]
		public NotifiableProperty<Color> TrackFaceDefaultColor { get; set; } = new(Color.black);

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
using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	[Description("游戏UI设置")]
	public class PlayfieldSetting : ISingletonSetting<PlayfieldSetting>
	{
		[Description("Tap和Hold与轨道边界的间隔大小")]
		public NotifiableProperty<float> TrackGap1 { get; set; } = new(0.15f);

		[Description("Slide与轨道边界的间隔大小")]
		public NotifiableProperty<float> TrackGap2 { get; set; } = new(0.3f);

		[Description("轨道默认颜色，该项也用来设置轨道的默认透明度")]
		public NotifiableProperty<Color> TrackFaceDefaultColor { get; set; } = new(Color.black);

		[Description("主相机位置")]
		public NotifiableProperty<Vector3> CameraPosition { get; set; } = new(Vector3.zero);

		[Description("主相机旋转角度")]
		public NotifiableProperty<Vector3> CameraRotation { get; set; } = new(Vector3.zero);

		[Description("Note下落时如果未被判定，则其视图层内容在判定时间之后延迟销毁的时间（非必要请勿修改）")]
		[HideInGame]
		public NotifiableProperty<int> TimeAfterEnd { get; set; } = new(2000);

		[Description("当Note在1速下首次位于该高度以下时才会真正生成其视图层内容（非必要请勿修改）")]
		public NotifiableProperty<float> UpperThreshold { get; set; } = new(15);

		[Description("当Note在1速下首次位于该高度以上时才会真正生成其视图层内容（非必要请勿修改）")]
		[HideInGame]
		public NotifiableProperty<float> LowerThreshold { get; set; } = new(-5);
	}
}
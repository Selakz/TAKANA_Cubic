using System.ComponentModel;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	public class PlayfieldSetting : ISingletonSetting<PlayfieldSetting>
	{
		[Description("初始速度")]
		public float Speed { get; set; } = 1.0f;

		[Description("当Note在1速下首次位于该高度以下时才会真正生成其视图层内容（非必要请勿修改）")]
		public float UpperThreshold { get; set; } = 8;

		[Description("当Note在1速下首次位于该高度以上时才会真正生成其视图层内容（非必要请勿修改）")]
		public float LowerThreshold { get; set; } = -5;

		[Description("Tap和Hold与轨道边界的间隔大小")]
		public float TrackGap1 { get; set; } = 0.15f;

		[Description("Slide与轨道边界的间隔大小")]
		public float TrackGap2 { get; set; } = 0.3f;

		[Description("Note下落时如果未被判定，则其视图层内容在判定时间之后延迟销毁的时间（非必要请勿修改）")]
		public int TimeAfterEnd { get; set; } = 2000;

		[Description("轨道默认颜色，该项也用来设置轨道的默认透明度")]
		public Color TrackFaceDefaultColor { get; set; } = Color.black;
	}
}
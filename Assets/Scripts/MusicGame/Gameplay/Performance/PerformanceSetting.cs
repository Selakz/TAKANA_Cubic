#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;
using UnityEngine;

namespace MusicGame.Gameplay.Performance
{
	[Description("Header")]
	public class PerformanceSetting : ISingletonSetting<PerformanceSetting>
	{
		[Description("UseDebugPanel")]
		public NotifiableProperty<bool> UseDebugPanel { get; set; } = new(false);

		[Description("UseVSync")]
		public NotifiableProperty<bool> UseVSync { get; set; } = new(true);

		[DropdownOptions(30, 60, 90, 120, 144)]
		[Description("TargetFrameRate")]
		public NotifiableProperty<int> TargetFrameRate { get; set; } = new(120)
			{ Clamp = value => Mathf.Clamp(value, 30, 144) };

		[DropdownOptions(0.25f, 0.5f, 0.75f, 1f)]
		[Description("ResolutionRatio")]
		public NotifiableProperty<float> ResolutionRatio { get; set; } = new(1f)
			{ Clamp = value => Mathf.Clamp(value, 0.25f, 1f) };

		[Description("MergeTrackFace")]
		public NotifiableProperty<bool> MergeTrackFace { get; set; } = new(false);

		[Description("EnableLaneBeam")]
		public NotifiableProperty<bool> EnableLaneBeam { get; set; } = new(true);
	}
}
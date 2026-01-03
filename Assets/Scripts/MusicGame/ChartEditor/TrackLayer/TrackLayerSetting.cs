using System.Collections.Generic;
using System.ComponentModel;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLayer
{
	[Description("Header")]
	public class TrackLayerSetting : ISingletonSetting<TrackLayerSetting>
	{
		[HideInGame]
		[Description("DefaultLayerId")]
		public NotifiableProperty<int> DefaultLayerId { get; set; } = new(0);

		[Description("DefaultLayerName")]
		public NotifiableProperty<string> DefaultLayerName { get; set; } = new("默认图层");

		[Description("ColorDefinitions")]
		[MaxLength(16)]
		public NotifiableProperty<List<Color?>> ColorDefinitions { get; set; } = new(new()
		{
			UnityParser.ParseHexAlphaTuple("(000000, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(303030, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(505050, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(808080, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(1F6B8E, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(2692C3, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(723D40, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(9D5155, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(682A6D, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(8C3993, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(BA4DC3, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(ED64F8, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(B06A35, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(B08F34, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(3F7625, 1.00)"),
			UnityParser.ParseHexAlphaTuple("(37A64C, 1.00)")
		});

		[Description("DefaultColor")]
		public NotifiableProperty<Color> DefaultColor { get; set; } = new(Color.black);

		[Description("SelectLayerOpacityRatio")]
		[MinValue(0)]
		[MaxValue(1)]
		public NotifiableProperty<float> SelectLayerOpacityRatio { get; set; } = new(1.0f);

		[Description("UnselectLayerOpacityRatio")]
		[MinValue(0)]
		[MaxValue(1)]
		public NotifiableProperty<float> UnselectLayerOpacityRatio { get; set; } = new(0.1f);
	}
}
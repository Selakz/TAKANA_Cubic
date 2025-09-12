using System.Collections.Generic;
using System.ComponentModel;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class TrackLayerSetting : ISingletonSetting<TrackLayerSetting>
	{
		[Description("默认图层的ID，一般情况下无需关注")]
		public int DefaultLayerId { get; set; } = 0;

		[Description("默认图层的名称")]
		public string DefaultLayerName { get; set; } = "默认图层";

		[Description("图层可用的颜色定义")]
		public List<Color> ColorDefinitions { get; set; } = new()
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
		};

		[Description("新建的图层默认使用的图层颜色")]
		public Color DefaultColor { get; set; } = Color.black;

		[Description("选中的图层的透明度倍率，范围为[0, 1]")]
		public float SelectLayerOpacityRatio { get; set; } = 1.0f;

		[Description("未选中的图层的透明度倍率，范围为[0, 1]")]
		public float UnselectLayerOpacityRatio { get; set; } = 0.1f;
	}
}
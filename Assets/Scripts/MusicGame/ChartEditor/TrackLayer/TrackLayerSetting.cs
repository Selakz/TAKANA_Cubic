using System.Collections.Generic;
using System.ComponentModel;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class TrackLayerSetting : ISingletonSetting<TrackLayerSetting>
	{
		[Description("Ĭ��ͼ���ID��һ������������ע")]
		public int DefaultLayerId { get; set; } = 0;

		[Description("Ĭ��ͼ�������")]
		public string DefaultLayerName { get; set; } = "Ĭ��ͼ��";

		[Description("ͼ����õ���ɫ����")]
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

		[Description("�½���ͼ��Ĭ��ʹ�õ�ͼ����ɫ")]
		public Color DefaultColor { get; set; } = Color.black;

		[Description("ѡ�е�ͼ���͸���ȱ��ʣ���ΧΪ[0, 1]")]
		public float SelectLayerOpacityRatio { get; set; } = 1.0f;

		[Description("δѡ�е�ͼ���͸���ȱ��ʣ���ΧΪ[0, 1]")]
		public float UnselectLayerOpacityRatio { get; set; } = 0.1f;
	}
}
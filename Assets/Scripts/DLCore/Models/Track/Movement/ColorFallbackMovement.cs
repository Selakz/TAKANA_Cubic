#nullable enable

using System;
using MusicGame.Models;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Movement;

namespace DLCore.Models.Track.Movement
{
	[ChartTypeMark("colorFallback")]
	public class ColorFallbackMovement : IMovement<ColorVariantGradient>, IChartSerializable
	{
		private readonly ColorVariant color;

		private static readonly ColorFallbackMovement[] instances =
			new ColorFallbackMovement[Enum.GetValues(typeof(ColorVariant)).Length];

		static ColorFallbackMovement()
		{
			for (int i = 0; i < instances.Length; i++)
			{
				instances[i] = new ColorFallbackMovement((ColorVariant)i);
			}
		}

		public ColorFallbackMovement(ColorVariant color) => this.color = color;

		public ColorVariantGradient GetPos(T3Time time) => new(color);

		public void Nudge(T3Time distance)
		{
		}

		public void Shift(ColorVariantGradient offset)
		{
		}

		public void Insert(T3Time time, ColorVariantGradient position)
		{
		}

		public static ColorFallbackMovement Instance(ColorVariant color)
		{
			return instances[(int)color];
		}

		public JObject GetSerializationToken() => new() { ["color"] = color.ToString() };

		public static ColorFallbackMovement Deserialize(JObject dict)
		{
			var color = Enum.Parse<ColorVariant>(dict.Get("color", ColorVariant.Gray.ToString()));
			return Instance(color);
		}
	}
}

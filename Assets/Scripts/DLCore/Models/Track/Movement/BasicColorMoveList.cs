#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Models;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Movement;
using UnityEngine;

namespace DLCore.Models.Track.Movement
{
	[ChartTypeMark("basicColorMoveList")]
	public class BasicColorMoveList : IMovement<ColorVariantGradient>, IChartSerializable
	{
		private const int DefaultGradientLengthMilli = 1000;

		private readonly SortedList<T3Time, ColorVariant> list = new();

		public int Count => list.Count;

		/// <summary>
		/// When switching to a color, the gradient fades from the previous color over this duration.
		/// </summary>
		public T3Time GradientLength { get; set; } = new(DefaultGradientLengthMilli);

		public ColorVariantGradient GetPos(T3Time time)
		{
			if (list.Count == 0) return new ColorVariantGradient(ColorVariant.Gray);
			if (time < list.Keys[0]) return new ColorVariantGradient(list.Values[0]);

			var search = list.BinarySearch(time);
			var currentIndex = search >= 0 ? search : ~search - 1;
			var currentColor = list.Values[currentIndex];
			if (currentIndex == 0) return new ColorVariantGradient(currentColor);

			var elapsed = time - list.Keys[currentIndex];
			if (elapsed >= GradientLength) return new ColorVariantGradient(currentColor);

			var gradient = Mathf.Clamp01(elapsed.Second / GradientLength.Second);
			return new ColorVariantGradient
			{
				FromColor = list.Values[currentIndex - 1],
				ToColor = currentColor,
				Gradient = gradient
			};
		}

		public void Nudge(T3Time distance)
		{
			var pairs = list.ToArray();
			list.Clear();
			foreach (var (time, color) in pairs) list.Add(time + distance, color);
		}

		public void Shift(ColorVariantGradient offset)
		{
		}

		public void Insert(T3Time time, ColorVariant color)
		{
			list[time] = color;
		}

		// The gradient is stored as its target color; the transition from the previous color is derived by GetPos.
		void IMovement<ColorVariantGradient>.Insert(T3Time time, ColorVariantGradient position)
		{
			Insert(time, position.ToColor);
		}

		public JObject GetSerializationToken()
		{
			var array = new JObject();
			foreach (var pair in list)
			{
				array[pair.Key.ToString()] = pair.Value.ToString();
			}

			var token = new JObject { ["list"] = array };
			token.AddIf("gradientLength", GradientLength.Milli, GradientLength.Milli != DefaultGradientLengthMilli);
			return token;
		}

		public static BasicColorMoveList Deserialize(JObject dict)
		{
			BasicColorMoveList result = new();
			result.GradientLength = dict.Get("gradientLength", DefaultGradientLengthMilli);
			if (dict["list"] is not JObject array) return result;
			foreach (var pair in array)
			{
				result.Insert(T3Time.Parse(pair.Key), Enum.Parse<ColorVariant>(pair.Value!.Value<string>()!));
			}

			return result;
		}
	}
}

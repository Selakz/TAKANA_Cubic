#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Static.Movement;

namespace MusicGame.Models.Track.Movement
{
	[ChartTypeMark("trackEdgeMovement")]
	public class TrackEdgeMovement : ITrackMovement
	{
		public TrackEdgeMovement(IMovement<float> leftEdgeMovement, IMovement<float> rightEdgeMovement)
		{
			Movement1 = leftEdgeMovement;
			Movement2 = rightEdgeMovement;
		}

		public TrackEdgeMovement(IEnumerable<V1EMoveItem> leftEdgeItems, IEnumerable<V1EMoveItem> rightEdgeItems)
		{
			Movement1 = new V1EMoveList(leftEdgeItems);
			Movement2 = new V1EMoveList(rightEdgeItems);
		}

		/// <summary> Left Edge </summary>
		public IMovement<float> Movement1 { get; set; }

		/// <summary> Right Edge </summary>
		public IMovement<float> Movement2 { get; set; }

		public float GetPos(T3Time time)
		{
			return (Movement1.GetPos(time) + Movement2.GetPos(time)) / 2;
		}

		public float GetWidth(T3Time time)
		{
			return Math.Abs(Movement1.GetPos(time) - Movement2.GetPos(time));
		}

		public void Nudge(T3Time distance)
		{
			Movement1.Nudge(distance);
			Movement2.Nudge(distance);
		}

		public void Shift(float offset)
		{
			Movement1.Shift(offset);
			Movement2.Shift(offset);
		}

		public JObject GetSerializationToken()
		{
			return new JObject()
			{
				["left"] = ((IChartSerializable)Movement1).Serialize(true),
				["right"] = ((IChartSerializable)Movement2).Serialize(true)
			};
		}

		public static TrackEdgeMovement Deserialize(JObject dict)
		{
			var leftList = (IMovement<float>)IChartSerializable.Deserialize((dict["left"] as JObject)!);
			var rightList = (IMovement<float>)IChartSerializable.Deserialize((dict["right"] as JObject)!);
			return new TrackEdgeMovement(leftList, rightList);
		}
	}
}
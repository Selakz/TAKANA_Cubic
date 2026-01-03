#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Static.Movement;

namespace MusicGame.Models.Track.Movement
{
	[ChartTypeMark("trackDirectMovement")]
	public class TrackDirectMovement : ITrackMovement
	{
		public TrackDirectMovement(IMovement<float> positionMovement, IMovement<float> widthMovement)
		{
			Movement1 = positionMovement;
			Movement2 = widthMovement;
		}

		public TrackDirectMovement(IEnumerable<V1EMoveItem> positionItems, IEnumerable<V1EMoveItem> widthItems)
		{
			Movement1 = new V1EMoveList(positionItems);
			Movement2 = new V1EMoveList(widthItems);
		}

		/// <summary> Position </summary>
		public IMovement<float> Movement1 { get; set; }

		/// <summary> Width </summary>
		public IMovement<float> Movement2 { get; set; }

		public float GetPos(T3Time time)
		{
			return Movement1.GetPos(time);
		}

		public float GetWidth(T3Time time)
		{
			return Movement2.GetPos(time);
		}

		public void Nudge(T3Time distance)
		{
			Movement1.Nudge(distance);
			Movement2.Nudge(distance);
		}

		public void Shift(float offset)
		{
			Movement1.Shift(offset);
		}

		public JObject GetSerializationToken()
		{
			return new JObject()
			{
				["position"] = ((IChartSerializable)Movement1).Serialize(true),
				["width"] = ((IChartSerializable)Movement2).Serialize(true),
			};
		}

		public static TrackDirectMovement Deserialize(JObject dict)
		{
			var positionList = (IMovement<float>)IChartSerializable.Deserialize((dict["position"] as JObject)!);
			var widthList = (IMovement<float>)IChartSerializable.Deserialize((dict["width"] as JObject)!);
			return new TrackDirectMovement(positionList, widthList);
		}
	}
}
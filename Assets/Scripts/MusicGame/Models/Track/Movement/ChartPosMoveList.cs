#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Static.Easing;
using T3Framework.Static.Movement;

namespace MusicGame.Models.Track.Movement
{
	[ChartTypeMark("position")]
	public class ChartPosMoveList : PositionMoveList<float>, IChartSerializable
	{
		public ChartPosMoveList()
		{
		}

		public ChartPosMoveList(IDictionary<T3Time, IPositionMoveItem<float>> items) : base(items)
		{
		}

		public override void Insert(T3Time time, float position)
		{
			Insert(time, new V1EMoveItem(position, Eases.Unmove));
		}

		public JObject GetSerializationToken()
		{
			var array = new JObject();
			foreach (var pair in this)
			{
				if (pair.Value is IStringSerializable item)
				{
					array[pair.Key.ToString()] = item.Serialize(true);
				}
			}

			var token = new JObject { ["list"] = array };
			return token;
		}

		public static ChartPosMoveList Deserialize(JObject dict)
		{
			if (dict["list"] is not JObject array) return new();
			Dictionary<T3Time, IPositionMoveItem<float>> list = new();
			foreach (var pair in array)
			{
				list.Add(T3Time.Parse(pair.Key),
					(IPositionMoveItem<float>)IStringSerializable.Deserialize(pair.Value!.Value<string>()!));
			}

			return new(list);
		}
	}
}
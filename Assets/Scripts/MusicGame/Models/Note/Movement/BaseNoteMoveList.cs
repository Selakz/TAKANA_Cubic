#nullable enable

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Static.Movement;
using UnityEngine;

namespace MusicGame.Models.Note.Movement
{
	[ChartTypeMark("baseNoteMoveList")]
	public class BaseNoteMoveList : INoteMovement, IMoveList<V1LSMoveItem>
	{
		private readonly V1LSMoveList moveList;
		private T3Time timeJudge;

		public T3Time TimeJudge
		{
			get => timeJudge;
			set
			{
				var distance = value - timeJudge;
				Nudge(distance);
			}
		}

		public int Count => moveList.Count;

		public BaseNoteMoveList(T3Time timeJudge)
		{
			this.timeJudge = timeJudge;
			moveList = new(timeJudge, 0);
		}

		public BaseNoteMoveList(IDictionary<T3Time, V1LSMoveItem> items, T3Time timeJudge)
		{
			this.timeJudge = timeJudge;
			moveList = new(items, timeJudge, 0);
		}

		public float GetPos(T3Time time) => -moveList.GetPos(time);

		public bool Insert(T3Time time, V1LSMoveItem item) => moveList.Insert(time, item);

		public bool Remove(T3Time time) => moveList.Remove(time);

		public bool Contains(V1LSMoveItem item) => moveList.Contains(item);

		public bool TryGet(T3Time time, out V1LSMoveItem item) => moveList.TryGet(time, out item);

		public void Nudge(T3Time distance)
		{
			timeJudge += distance;
			moveList.Nudge(distance);
		}

		/// <summary> Note's movement should not be shifted, since it should be 0 when judge time. </summary>
		public void Shift(float offset)
		{
		}

		public T3Time FirstTimeWhen(float edge, bool lesserThan)
		{
			for (int i = 0; i < moveList.Count; i++)
			{
				float pos = GetPos(moveList[i].Key);
				if (lesserThan ? pos <= edge : pos >= edge)
				{
					if (i == 0)
					{
						if (lesserThan ? moveList[0].Value.Speed <= 0 : moveList[0].Value.Speed >= 0)
						{
							return T3Time.MinValue;
						}

						return moveList[0].Key - (edge - pos) / moveList[0].Value.Speed;
					}

					float lastPos = GetPos(moveList[i - 1].Key);
					float t = (edge - lastPos) / (pos - lastPos);
					return Mathf.Lerp(moveList[i - 1].Key, moveList[i].Key, t);
				}
			}

			if (lesserThan ? moveList[^1].Value.Speed <= 0 : moveList[^1].Value.Speed >= 0) return T3Time.MaxValue;
			return moveList[^1].Key + (GetPos(moveList[^1].Key) - edge) / moveList[^1].Value.Speed;
		}

		public bool IsDefault()
		{
			return Count == 1 && Mathf.Abs(moveList[0].Key - TimeJudge) <= 1 &&
			       Mathf.Approximately(moveList[0].Value.Speed, 1f);
		}

		public IEnumerator<KeyValuePair<T3Time, V1LSMoveItem>> GetEnumerator() => moveList.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public JObject GetSerializationToken()
		{
			var baseToken = moveList.GetSerializationToken();
			var token = new JObject
			{
				["timeJudge"] = TimeJudge.Milli,
				["list"] = baseToken["list"]
			};
			return token;
		}

		public static BaseNoteMoveList Deserialize(JObject dict)
		{
			T3Time timeJudge = dict["timeJudge"]!.Value<int>();
			if (dict["list"] is not JObject array) return new(timeJudge);
			Dictionary<T3Time, V1LSMoveItem> items = new();
			foreach (var pair in array)
			{
				items.Add(T3Time.Parse(pair.Key), V1LSMoveItem.Deserialize(pair.Value!.Value<string>()!));
			}

			return new BaseNoteMoveList(items, timeJudge);
		}
	}
}
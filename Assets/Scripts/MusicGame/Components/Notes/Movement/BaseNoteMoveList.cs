using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Components.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using UnityEngine;

namespace MusicGame.Components.Notes.Movement
{
	// TODO: FIXME: A little weird if having speed variation.
	public class BaseNoteMoveList : INoteMovement, IMoveList<V1LSMoveItem>
	{
		public static string TypeMark => "baseNoteMoveList";

		private readonly V1LSMoveList moveList;
		private T3Time timeJudge;

		public T3Time TimeJudge
		{
			get => timeJudge;
			set
			{
				timeJudge = value;
				moveList.BaseTime = timeJudge;
			}
		}

		public event Action OnMovementUpdated = delegate { };

		public int Count => moveList.Count;

		public BaseNoteMoveList(T3Time timeJudge)
		{
			this.timeJudge = timeJudge;
			moveList = new(timeJudge, 0);
			moveList.OnMovementUpdated += OnMovementUpdated.Invoke;
		}

		public BaseNoteMoveList(IEnumerable<V1LSMoveItem> items, T3Time timeJudge)
		{
			this.timeJudge = timeJudge;
			moveList = new(items, timeJudge, 0);
		}

		public float GetPos(T3Time time)
		{
			return -moveList.GetPos(time);
		}

		public bool Insert(V1LSMoveItem item)
		{
			return moveList.Insert(item);
		}

		public bool Remove(T3Time time)
		{
			return moveList.Remove(time);
		}

		public bool Contains(V1LSMoveItem item)
		{
			return moveList.Contains(item);
		}

		public bool TryGet(T3Time time, out V1LSMoveItem item)
		{
			return moveList.TryGet(time, out item);
		}

		public IMovement<float> Clone(T3Time timeOffset, float positionOffset)
		{
			return new BaseNoteMoveList(
				moveList.Clone(timeOffset, positionOffset) as V1LSMoveList, TimeJudge + timeOffset);
		}

		public T3Time FirstTimeWhen(float edge, bool lesserThan)
		{
			for (int i = 0; i < moveList.Count; i++)
			{
				float pos = GetPos(moveList[i].Time);
				if (lesserThan ? pos <= edge : pos >= edge)
				{
					if (i == 0)
					{
						if (lesserThan ? moveList[0].Speed <= 0 : moveList[0].Speed >= 0)
						{
							return T3Time.MinValue;
						}

						return moveList[0].Time - (edge - pos) / moveList[0].Speed;
					}

					float lastPos = GetPos(moveList[i - 1].Time);
					float t = (edge - lastPos) / (pos - lastPos);
					return Mathf.Lerp(moveList[i - 1].Time, moveList[i].Time, t);
				}
			}

			if (lesserThan ? moveList[^1].Speed <= 0 : moveList[^1].Speed >= 0) return T3Time.MaxValue;
			return moveList[^1].Time + (GetPos(moveList[^1].Time) - edge) / moveList[^1].Speed;
		}

		public bool IsDefault()
		{
			return Count == 1 && Mathf.Abs(moveList[0].Time - TimeJudge) <= 1 &&
			       Mathf.Approximately(moveList[0].Speed, 1f);
		}

		public IEnumerator<V1LSMoveItem> GetEnumerator()
		{
			return moveList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public JToken GetSerializationToken()
		{
			var baseToken = moveList.GetSerializationToken();
			var token = new JObject
			{
				["timeJudge"] = TimeJudge.Milli,
				["list"] = baseToken["list"]
			};
			return token;
		}

		public static BaseNoteMoveList Deserialize(JToken token)
		{
			if (token is not JContainer container) return default;
			T3Time timeJudge = container["timeJudge"]!.Value<int>();
			var list = container["list"]!.Select(a => V1LSMoveItem.Parse(a.Value<string>()));
			return new BaseNoteMoveList(list, timeJudge);
		}
	}
}
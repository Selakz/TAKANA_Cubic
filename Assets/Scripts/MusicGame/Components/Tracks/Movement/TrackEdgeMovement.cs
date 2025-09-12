using System;
using System.Collections.Generic;
using MusicGame.Components.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;

namespace MusicGame.Components.Tracks.Movement
{
	public class TrackEdgeMovement : ITrackMovement
	{
		public static string TypeMark => "trackEdgeMoveList";

		public TrackEdgeMovement(IMovement<float> leftEdgeMovement, IMovement<float> rightEdgeMovement)
		{
			Movement1 = leftEdgeMovement;
			Movement2 = rightEdgeMovement;
			Movement1.OnMovementUpdated += () => OnTrackMovementUpdated.Invoke();
			Movement2.OnMovementUpdated += () => OnTrackMovementUpdated.Invoke();
		}

		public TrackEdgeMovement(IEnumerable<V1EMoveItem> leftEdgeItems, IEnumerable<V1EMoveItem> rightEdgeItems)
		{
			Movement1 = new V1EMoveList(leftEdgeItems);
			Movement2 = new V1EMoveList(rightEdgeItems);
			Movement1.OnMovementUpdated += () => OnTrackMovementUpdated.Invoke();
			Movement2.OnMovementUpdated += () => OnTrackMovementUpdated.Invoke();
		}

		public event Action OnTrackMovementUpdated = delegate { };

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

		public ITrackMovement Clone(T3Time timeOffset, float positionOffset)
		{
			var moveList1 = Movement1.Clone(timeOffset, positionOffset);
			var moveList2 = Movement2.Clone(timeOffset, positionOffset);
			return new TrackEdgeMovement(moveList1, moveList2);
		}

		public JToken GetSerializationToken()
		{
			return new JObject()
			{
				["left"] = Movement1.Serialize(true),
				["right"] = Movement2.Serialize(true)
			};
		}

		public static TrackEdgeMovement Deserialize(JToken token)
		{
			if (token is not JContainer container) return default;
			var leftList = (IMovement<float>)ISerializable.Deserialize(container["left"]);
			var rightList = (IMovement<float>)ISerializable.Deserialize(container["right"]);
			return new TrackEdgeMovement(leftList, rightList);
		}
	}
}
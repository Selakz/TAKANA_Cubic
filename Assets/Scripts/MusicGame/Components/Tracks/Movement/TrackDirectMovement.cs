using System;
using System.Collections.Generic;
using MusicGame.Components.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;

namespace MusicGame.Components.Tracks.Movement
{
	public class TrackDirectMovement : ITrackMovement
	{
		public static string TypeMark => "trackDirectMoveList";

		public TrackDirectMovement(IMovement<float> positionMovement, IMovement<float> widthMovement)
		{
			Movement1 = positionMovement;
			Movement2 = widthMovement;
			Movement1.OnMovementUpdated += () => OnTrackMovementUpdated.Invoke();
			Movement2.OnMovementUpdated += () => OnTrackMovementUpdated.Invoke();
		}

		public TrackDirectMovement(IEnumerable<V1EMoveItem> positionItems, IEnumerable<V1EMoveItem> widthItems)
		{
			Movement1 = new V1EMoveList(positionItems);
			Movement2 = new V1EMoveList(widthItems);
			Movement1.OnMovementUpdated += () => OnTrackMovementUpdated.Invoke();
			Movement2.OnMovementUpdated += () => OnTrackMovementUpdated.Invoke();
		}

		public event Action OnTrackMovementUpdated = delegate { };

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

		public ITrackMovement Clone(T3Time timeOffset, float positionOffset)
		{
			var moveList1 = Movement1.Clone(timeOffset, positionOffset);
			var moveList2 = Movement2.Clone(timeOffset, positionOffset);
			return new TrackDirectMovement(moveList1, moveList2);
		}

		public JToken GetSerializationToken()
		{
			return new JObject()
			{
				["position"] = Movement1.Serialize(true),
				["width"] = Movement2.Serialize(true),
			};
		}

		public static TrackDirectMovement Deserialize(JToken token)
		{
			if (token is not JContainer container) return default;
			var positionList = (IMovement<float>)ISerializable.Deserialize(container["position"]);
			var widthList = (IMovement<float>)ISerializable.Deserialize(container["width"]);
			return new TrackDirectMovement(positionList, widthList);
		}
	}
}
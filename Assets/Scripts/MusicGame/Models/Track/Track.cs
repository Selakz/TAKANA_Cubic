#nullable enable

using MusicGame.Models.Track.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;

namespace MusicGame.Models.Track
{
	[ChartTypeMark("track")]
	public class Track : ITrack
	{
		public T3Time TimeStart { get; set; }

		public T3Time TimeEnd { get; set; }

		public ITrackMovement Movement { get; set; }

		public ModelProperty Properties { get; set; } = new();

		public ModelProperty EditorConfig { get; set; } = new();

		public T3Time TimeMin => TimeStart;
		public T3Time TimeMax => TimeEnd;

		public Track(T3Time timeStart, T3Time timeEnd)
		{
			TimeStart = timeStart;
			TimeEnd = timeEnd;
			Movement = TrackFallbackMovement.Instance;
		}

		public void Nudge(T3Time distance)
		{
			TimeStart += distance;
			TimeEnd += distance;
			Movement.Nudge(distance);
		}

		public void Shift(float offset)
		{
			Movement.Shift(offset);
		}

		public JObject GetSerializationToken()
		{
			var token = new JObject();
			token.Add("timeStart", TimeStart.Milli);
			token.Add("timeEnd", TimeEnd.Milli);
			token.Add("movement", Movement.Serialize(true));
			token.AddProperties(this);
			return token;
		}

		public static Track Deserialize(JObject dict)
		{
			T3Time timeStart = dict["timeStart"]!.Value<int>();
			T3Time timeEnd = dict["timeEnd"]!.Value<int>();
			var track = new Track(timeStart, timeEnd)
			{
				Movement = dict.TryGetValue("movement", out var movementToken)
					? (ITrackMovement)IChartSerializable.Deserialize((movementToken as JObject)!)
					: TrackFallbackMovement.Instance
			};
			track.SetProperties(dict);
			return track;
		}
	}
}
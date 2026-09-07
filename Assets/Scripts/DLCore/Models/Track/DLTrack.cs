#nullable enable

using DLCore.Models.Track.Movement;
using MusicGame.Models;
using MusicGame.Models.Track.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Static.Movement;

namespace DLCore.Models.Track
{
	[ChartTypeMark("dlTrack")]
	public class DLTrack : MusicGame.Models.Track.Track, IDLTrack
	{
		public DLTrack(T3Time timeStart, T3Time timeEnd) : base(timeStart, timeEnd)
		{
			ColorMovement = ColorFallbackMovement.Instance(ColorVariant.Gray);
		}

		public IMovement<ColorVariantGradient> ColorMovement { get; set; }

		public override void Nudge(T3Time distance)
		{
			base.Nudge(distance);
			ColorMovement.Nudge(distance);
		}

		public override JObject GetSerializationToken()
		{
			var dict = base.GetSerializationToken();
			dict["colorMovement"] = ((IChartSerializable)ColorMovement).Serialize(true);
			return dict;
		}

		public new static DLTrack Deserialize(JObject dict)
		{
			T3Time timeStart = dict["timeStart"]!.Value<int>();
			T3Time timeEnd = dict["timeEnd"]!.Value<int>();
			var track = new DLTrack(timeStart, timeEnd)
			{
				Movement = dict.TryGetValue("movement", out var movementToken)
					? (ITrackMovement)IChartSerializable.Deserialize((movementToken as JObject)!)
					: TrackFallbackMovement.Instance,
				ColorMovement = dict.TryGetValue("colorMovement", out var colorMovementToken)
					? (IMovement<ColorVariantGradient>)IChartSerializable.Deserialize((colorMovementToken as JObject)!)
					: ColorFallbackMovement.Instance(ColorVariant.Gray)
			};
			track.SetProperties(dict);
			return track;
		}
	}
}

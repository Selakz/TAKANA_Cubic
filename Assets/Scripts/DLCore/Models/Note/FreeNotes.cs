#nullable enable

using System;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Note.Movement;
using MusicGame.Models.Track.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;

namespace DLCore.Models.Note
{
	public interface IDLFreeNote : IDLNote
	{
		public ITrackMovement HorizontalMovement { get; set; }
	}

	[ChartTypeMark("dlFreeHit")]
	public class DLFreeHit : DLHit, IDLFreeNote
	{
		public ITrackMovement HorizontalMovement { get; set; }

		public DLFreeHit(T3Time timeJudge, HitType type, ColorVariant color) : base(timeJudge, type, color)
		{
			HorizontalMovement = TrackFallbackMovement.Instance;
		}

		public override void Nudge(T3Time distance)
		{
			base.Nudge(distance);
			HorizontalMovement.Nudge(distance);
		}

		public override JObject GetSerializationToken()
		{
			var dict = base.GetSerializationToken();
			dict["horizontalMovement"] = HorizontalMovement.Serialize(true);
			return dict;
		}

		public new static DLFreeHit Deserialize(JObject dict)
		{
			T3Time timeJudge = dict["timeJudge"]!.Value<int>();
			HitType type = Enum.Parse<HitType>(dict.Get("hitType", HitType.Tap.ToString()));
			ColorVariant color = Enum.Parse<ColorVariant>(dict.Get("color", ColorVariant.Gray.ToString()));
			var hit = new DLFreeHit(timeJudge, type, color)
			{
				Movement = dict.TryGetValue("movement", out var movementToken)
					? (INoteMovement)IChartSerializable.Deserialize((movementToken as JObject)!)
					: new BaseNoteMoveList(timeJudge),
				HorizontalMovement = dict.TryGetValue("horizontalMovement", out var horizontalMovementToken)
					? (ITrackMovement)IChartSerializable.Deserialize((horizontalMovementToken as JObject)!)
					: TrackFallbackMovement.Instance
			};
			hit.SetProperties(dict);
			return hit;
		}
	}

	[ChartTypeMark("dlFreeHold")]
	public class DLFreeHold : DLHold, IDLFreeNote
	{
		public ITrackMovement HorizontalMovement { get; set; }

		public DLFreeHold(T3Time timeJudge, T3Time timeEnd, ColorVariant color) : base(timeJudge, timeEnd, color)
		{
			HorizontalMovement = TrackFallbackMovement.Instance;
		}

		public override void Nudge(T3Time distance)
		{
			base.Nudge(distance);
			HorizontalMovement.Nudge(distance);
		}

		public override JObject GetSerializationToken()
		{
			var dict = base.GetSerializationToken();
			dict["horizontalMovement"] = HorizontalMovement.Serialize(true);
			return dict;
		}

		public new static DLFreeHold Deserialize(JObject dict)
		{
			T3Time timeJudge = dict["timeJudge"]!.Value<int>();
			T3Time timeEnd = dict["timeEnd"]!.Value<int>();
			ColorVariant color = Enum.Parse<ColorVariant>(dict.Get("color", ColorVariant.Gray.ToString()));
			var hold = new DLFreeHold(timeJudge, timeEnd, color)
			{
				Movement = dict.TryGetValue("movement", out var movementToken)
					? (INoteMovement)IChartSerializable.Deserialize((movementToken as JObject)!)
					: new BaseNoteMoveList(timeJudge),
				TailMovement = dict.TryGetValue("tailMovement", out var tailMovementToken)
					? (INoteMovement)IChartSerializable.Deserialize((tailMovementToken as JObject)!)
					: new BaseNoteMoveList(timeEnd),
				HorizontalMovement = dict.TryGetValue("horizontalMovement", out var horizontalMovementToken)
					? (ITrackMovement)IChartSerializable.Deserialize((horizontalMovementToken as JObject)!)
					: TrackFallbackMovement.Instance
			};
			hold.SetProperties(dict);
			return hold;
		}
	}
}

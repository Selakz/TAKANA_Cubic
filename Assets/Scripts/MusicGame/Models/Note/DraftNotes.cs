#nullable enable

using System;
using MusicGame.Models.Note.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;

namespace MusicGame.Models.Note
{
	public interface ISolitaryNote : INote
	{
		public float Position { get; set; }

		public float Width { get; set; }

		public INote ToSimpleNote();
	}

	[ChartTypeMark("draftHit")]
	public class DraftHit : Hit, ISolitaryNote
	{
		public float Position { get; set; }

		public float Width { get; set; }

		public DraftHit(T3Time timeJudge, HitType type, float position, float width) : base(timeJudge, type)
		{
			Position = position;
			Width = width;
		}

		public INote ToSimpleNote() => new Hit(TimeJudge, Type)
		{
			Movement = IChartSerializable.Clone(Movement),
			EditorConfig = IChartSerializable.Clone(EditorConfig),
			Properties = IChartSerializable.Clone(Properties)
		};

		public override JObject GetSerializationToken()
		{
			var dict = base.GetSerializationToken();
			dict.Add("position", Position);
			dict.Add("width", Width);
			return dict;
		}

		public new static DraftHit Deserialize(JObject dict)
		{
			T3Time timeJudge = dict["timeJudge"]!.Value<int>();
			HitType type = Enum.Parse<HitType>(dict.Get("hitType", HitType.Tap.ToString()));
			float position = dict["position"]!.Value<float>();
			float width = dict["width"]!.Value<float>();
			var hit = new DraftHit(timeJudge, type, position, width)
			{
				Movement = dict.TryGetValue("movement", out var movementToken)
					? (INoteMovement)IChartSerializable.Deserialize((movementToken as JObject)!)
					: new BaseNoteMoveList(timeJudge)
			};
			hit.SetProperties(dict);
			return hit;
		}
	}

	[ChartTypeMark("draftHold")]
	public class DraftHold : Hold, ISolitaryNote
	{
		public float Position { get; set; }

		public float Width { get; set; }

		public DraftHold(T3Time timeJudge, T3Time timeEnd, float position, float width) : base(timeJudge, timeEnd)
		{
			Position = position;
			Width = width;
		}

		public INote ToSimpleNote() => new Hold(TimeJudge, TimeEnd)
		{
			Movement = IChartSerializable.Clone(Movement),
			TailMovement = IChartSerializable.Clone(TailMovement),
			EditorConfig = IChartSerializable.Clone(EditorConfig),
			Properties = IChartSerializable.Clone(Properties)
		};

		public override JObject GetSerializationToken()
		{
			var dict = base.GetSerializationToken();
			dict.Add("position", Position);
			dict.Add("width", Width);
			return dict;
		}

		public new static DraftHold Deserialize(JObject dict)
		{
			T3Time timeJudge = dict["timeJudge"]!.Value<int>();
			T3Time timeEnd = dict["timeEnd"]!.Value<int>();
			float position = dict["position"]!.Value<float>();
			float width = dict["width"]!.Value<float>();
			var hold = new DraftHold(timeJudge, timeEnd, position, width)
			{
				Movement = dict.TryGetValue("movement", out var movementToken)
					? (INoteMovement)IChartSerializable.Deserialize((movementToken as JObject)!)
					: new BaseNoteMoveList(timeJudge),
				TailMovement = dict.TryGetValue("tailMovement", out var tailMovementToken)
					? (INoteMovement)IChartSerializable.Deserialize((tailMovementToken as JObject)!)
					: new BaseNoteMoveList(timeEnd)
			};
			hold.SetProperties(dict);
			return hold;
		}
	}
}
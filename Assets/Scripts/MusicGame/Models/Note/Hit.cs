#nullable enable

using System;
using MusicGame.Models.Note.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;

namespace MusicGame.Models.Note
{
	public enum HitType
	{
		Tap,
		Slide
	}

	[ChartTypeMark("hit")]
	public class Hit : INote
	{
		public HitType Type { get; set; }

		public T3Time TimeJudge { get; set; }

		public INoteMovement Movement { get; set; }

		public ModelProperty Properties { get; set; } = new();

		public ModelProperty EditorConfig { get; set; } = new();

		public T3Time TimeMin => TimeJudge;
		public T3Time TimeMax => TimeJudge;

		public Hit(T3Time timeJudge, HitType type)
		{
			TimeJudge = timeJudge;
			Type = type;
			Movement = new BaseNoteMoveList(timeJudge);
		}

		public void Nudge(T3Time distance)
		{
			var newTimeJudge = TimeJudge + distance;
			TimeJudge = newTimeJudge;
			Movement.Nudge(distance);
		}

		public JObject GetSerializationToken()
		{
			var dict = new JObject();
			dict.Add("timeJudge", TimeJudge.Milli);
			dict.Add("hitType", Type.ToString());
			dict.AddIf("movement", Movement.Serialize(true),
				!(Movement is BaseNoteMoveList baseNoteMoveList && baseNoteMoveList.IsDefault()));
			dict.AddProperties(this);
			return dict;
		}

		public static Hit Deserialize(JObject dict)
		{
			T3Time timeJudge = dict["timeJudge"]!.Value<int>();
			HitType type = Enum.Parse<HitType>(dict.Get("hitType", HitType.Tap.ToString()));
			var hit = new Hit(timeJudge, type)
			{
				Movement = dict.TryGetValue("movement", out var movementToken)
					? (INoteMovement)IChartSerializable.Deserialize((movementToken as JObject)!)
					: new BaseNoteMoveList(timeJudge)
			};
			hit.SetProperties(dict);
			return hit;
		}
	}
}
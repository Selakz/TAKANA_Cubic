#nullable enable

using System;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Note.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;

namespace DLCore.Models.Note
{
	[ChartTypeMark("dlHit")]
	public class DLHit : Hit, IDLNote
	{
		public DLHit(T3Time timeJudge, HitType type, ColorVariant color) : base(timeJudge, type)
		{
			Color = color;
		}

		public ColorVariant Color { get; set; }

		public override JObject GetSerializationToken()
		{
			var dict = base.GetSerializationToken();
			dict["color"] = Color.ToString();
			return dict;
		}

		public new static DLHit Deserialize(JObject dict)
		{
			T3Time timeJudge = dict["timeJudge"]!.Value<int>();
			HitType type = Enum.Parse<HitType>(dict.Get("hitType", HitType.Tap.ToString()));
			ColorVariant color = Enum.Parse<ColorVariant>(dict.Get("color", ColorVariant.Gray.ToString()));
			var hit = new DLHit(timeJudge, type, color)
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

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
	[ChartTypeMark("dlHold")]
	public class DLHold : Hold, IDLNote
	{
		public DLHold(T3Time timeJudge, T3Time timeEnd, ColorVariant color) : base(timeJudge, timeEnd)
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

		public new static DLHold Deserialize(JObject dict)
		{
			T3Time timeJudge = dict["timeJudge"]!.Value<int>();
			T3Time timeEnd = dict["timeEnd"]!.Value<int>();
			ColorVariant color = Enum.Parse<ColorVariant>(dict.Get("color", ColorVariant.Gray.ToString()));
			var hold = new DLHold(timeJudge, timeEnd, color)
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

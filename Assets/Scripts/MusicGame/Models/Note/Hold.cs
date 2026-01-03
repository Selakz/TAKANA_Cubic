#nullable enable

using MusicGame.Models.Note.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;

namespace MusicGame.Models.Note
{
	[ChartTypeMark("hold")]
	public class Hold : INote
	{
		public T3Time TimeJudge { get; set; }

		public T3Time TimeEnd { get; set; }

		public T3Time Length
		{
			get => TimeEnd - TimeJudge;
			set => TimeEnd = TimeJudge + value;
		}

		public INoteMovement Movement { get; set; }

		public INoteMovement TailMovement { get; set; }

		public ModelProperty Properties { get; set; } = new();

		public ModelProperty EditorConfig { get; set; } = new();

		public T3Time TimeMin => TimeJudge;
		public T3Time TimeMax => TimeEnd;

		public Hold(T3Time timeJudge, T3Time timeEnd)
		{
			TimeJudge = timeJudge;
			TimeEnd = timeEnd;
			Movement = new BaseNoteMoveList(timeJudge);
			TailMovement = new BaseNoteMoveList(timeEnd);
		}

		public void Nudge(T3Time distance)
		{
			TimeJudge += distance;
			Movement.Nudge(distance);
			TimeEnd += distance;
			TailMovement.Nudge(distance);
		}

		public void NudgeJudge(T3Time distance)
		{
			TimeJudge += distance;
			Movement.Nudge(distance);
		}

		public void NudgeEnd(T3Time distance)
		{
			TimeEnd += distance;
			TailMovement.Nudge(distance);
		}

		public JObject GetSerializationToken()
		{
			var dict = new JObject();
			dict.Add("timeJudge", TimeJudge.Milli);
			dict.Add("timeEnd", TimeEnd.Milli);
			dict.AddIf("movement", Movement.Serialize(true),
				!(Movement is BaseNoteMoveList baseHeadMoveList && baseHeadMoveList.IsDefault()));
			dict.AddIf("tailMovement", TailMovement.Serialize(true),
				!(TailMovement is BaseNoteMoveList baseTailMoveList && baseTailMoveList.IsDefault()));
			dict.AddProperties(this);
			return dict;
		}

		public static Hold Deserialize(JObject dict)
		{
			T3Time timeJudge = dict["timeJudge"]!.Value<int>();
			T3Time timeEnd = dict["timeEnd"]!.Value<int>();
			var hold = new Hold(timeJudge, timeEnd)
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
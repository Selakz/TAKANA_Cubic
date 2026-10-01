#nullable enable

using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using UnityEngine;

namespace MusicGame.Models.JudgeLine
{
	[ChartTypeMark("line")]
	public class StaticJudgeLine : IJudgeLine
	{
		public float Opacity { get; set; } = 1f;

		public ModelProperty Properties { get; set; } = new();

		public ModelProperty EditorConfig { get; set; } = new();

		public T3Time TimeMin => T3Time.MinValue + 1;
		public T3Time TimeMax => T3Time.MaxValue - 1;

		public void Nudge(T3Time distance)
		{
		}

		public JObject GetSerializationToken()
		{
			var dict = new JObject();
			dict.AddIf("opacity", Opacity, !Mathf.Approximately(Opacity, 1f));
			dict.AddProperties(this);
			return dict;
		}

		public static StaticJudgeLine Deserialize(JObject dict)
		{
			var line = new StaticJudgeLine();
			line.Opacity = dict.Get("opacity", 1f);
			line.SetProperties(dict);
			return line;
		}
	}
}
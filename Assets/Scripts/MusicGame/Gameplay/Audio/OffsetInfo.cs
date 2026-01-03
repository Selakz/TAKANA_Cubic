#nullable enable

using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;

namespace MusicGame.Gameplay.Audio
{
	[ChartTypeMark("offset")]
	public class OffsetInfo : SingleValueProperty<T3Time>
	{
		public OffsetInfo() => Value = 0;

		public OffsetInfo(T3Time value) => Value = value;

		protected override T3Time FromJToken(JToken? token) => token?.Value<int>() ?? 0;

		protected override JToken ToJToken(T3Time value) => value.Milli;

		public static OffsetInfo Deserialize(JObject dict) => Deserialize<OffsetInfo>(dict);
	}

	public static class OffsetInfoExtensions
	{
		public static OffsetInfo GetsOffsetInfo(this ChartInfo chart)
		{
			var info = chart.Properties.Get<OffsetInfo>("offset");
			if (info is not null) return info;
			info = new OffsetInfo(0);
			chart.Properties["offset"] = info;
			return info;
		}

		public static void SetOffsetInfo(this ChartInfo chart, T3Time offset)
		{
			var info = chart.Properties.Get<OffsetInfo>("offset");
			if (info is null && offset == 0) return;
			info = new OffsetInfo(offset);
			chart.Properties["offset"] = info;
		}
	}
}
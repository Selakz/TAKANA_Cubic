#nullable enable

using MusicGame.Models;
using MusicGame.Models.Track;
using Newtonsoft.Json.Linq;

namespace MusicGame.Utility.Dakumi
{
	[ChartTypeMark("dakumi_sw0")]
	public class ShowWhen0Flag : SingleValueProperty<bool>
	{
		protected override bool FromJToken(JToken? token) => token?.Value<bool>() ?? true;

		protected override JToken ToJToken(bool value) => value;

		public static ShowWhen0Flag Deserialize(JObject dict)
			=> Deserialize<ShowWhen0Flag>(dict);
	}

	public static class ShowWhen0FlagExtensions
	{
		public static bool IsShowWhen0(this ITrack model)
		{
			var flag = model.Properties.Get<ShowWhen0Flag>("showWhen0");
			return flag is null || flag.Value;
		}

		public static void SetIsShowWhen0(this ITrack model, bool value)
		{
			var flag = model.Properties.Get<ShowWhen0Flag>("showWhen0");
			if (flag is null)
			{
				if (value) return;
				else
				{
					flag = new ShowWhen0Flag();
					model.Properties["showWhen0"] = flag;
				}
			}

			flag.Value = value;
		}
	}
}
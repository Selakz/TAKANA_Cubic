#nullable enable

using Newtonsoft.Json.Linq;

namespace MusicGame.Models.Note
{
	[ChartTypeMark("dummyFlag")]
	public class DummyFlag : SingleValueProperty<bool>
	{
		protected override bool FromJToken(JToken? token) => token?.Value<bool>() ?? false;

		protected override JToken ToJToken(bool value) => value;

		public static DummyFlag Deserialize(JObject dict) => Deserialize<DummyFlag>(dict);
	}

	public static class DummyFlagExtensions
	{
		public static bool IsDummy(this INote note)
		{
			var flag = note.Properties.Get<DummyFlag>("isDummy");
			return flag is not null && flag.Value;
		}

		public static void SetDummy(this INote note, bool value)
		{
			var flag = note.Properties.Get<DummyFlag>("isDummy");
			if (flag is null)
			{
				if (!value) return;
				else
				{
					flag = new DummyFlag();
					note.Properties["isDummy"] = flag;
				}
			}

			flag.Value = value;
		}
	}
}
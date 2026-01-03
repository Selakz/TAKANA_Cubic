#nullable enable

using Newtonsoft.Json.Linq;

namespace MusicGame.Models
{
	[ChartTypeMark("editorOnly")]
	public class EditorOnlyFlag : SingleValueProperty<bool>
	{
		protected override bool FromJToken(JToken? token) => token?.Value<bool>() ?? false;

		protected override JToken ToJToken(bool value) => value;

		public static EditorOnlyFlag Deserialize(JObject dict)
			=> SingleValueProperty<bool>.Deserialize<EditorOnlyFlag>(dict);
	}

	public static class EditorOnlyFlagExtensions
	{
		public static bool IsEditorOnly(this IChartModel model)
		{
			var flag = model.Properties.Get<EditorOnlyFlag>("isEditorOnly");
			return flag is not null && flag.Value;
		}

		public static void SetIsEditorOnly(this IChartModel model, bool value)
		{
			var flag = model.Properties.Get<EditorOnlyFlag>("isEditorOnly");
			if (flag is null)
			{
				if (!value) return;
				else
				{
					flag = new EditorOnlyFlag();
					model.Properties["isEditorOnly"] = flag;
				}
			}

			flag.Value = value;
		}
	}
}
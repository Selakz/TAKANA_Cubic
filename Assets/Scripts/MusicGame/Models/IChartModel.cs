#nullable enable

using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;

namespace MusicGame.Models
{
	public interface IChartModel : IChartSerializable
	{
		public T3Time TimeMin { get; }

		public T3Time TimeMax { get; }

		public ModelProperty Properties { get; set; }

		public ModelProperty EditorConfig { get; set; }

		public void Nudge(T3Time distance);
	}

	public static class ChartModelExtensions
	{
		public static void AddProperties(this JObject dict, IChartModel model)
		{
			dict.AddIf("properties", model.Properties.GetSerializationToken(), model.Properties.Count > 0);
			dict.AddIf("editorconfig", model.EditorConfig.GetSerializationToken(), model.EditorConfig.Count > 0);
		}

		public static void SetProperties(this IChartModel model, JObject dict)
		{
			model.Properties = dict.TryGetValue("properties", out JObject propertiesToken)
				? ModelProperty.Deserialize(propertiesToken)
				: new();
			model.EditorConfig = dict.TryGetValue("editorconfig", out JObject configToken)
				? ModelProperty.Deserialize(configToken)
				: new();
		}
	}
}
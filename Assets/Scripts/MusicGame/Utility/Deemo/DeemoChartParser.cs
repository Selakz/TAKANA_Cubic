#nullable enable

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MusicGame.Utility.Deemo
{
	public static class DeemoChartParser
	{
		public static DeemoChart? Parse(string json)
		{
			// Try standard format (DeemoV2/IIV2 with Newtonsoft $id/$ref)
			try
			{
				var chart = JsonConvert.DeserializeObject<DeemoChart>(json);
				if (chart is { Notes.Count: > 0 })
					return chart;
			}
			catch (Exception ex)
			{
				Debug.Log($"Failed to parse chart in V2/IIV2 format: {ex.Message}");
				try
				{
					// Try V3 format
					return ParseV3(json);
				}
				catch (Exception v3Ex)
				{
					Debug.LogError($"Failed to parse chart in V3 format: {v3Ex.Message}");
					return null;
				}
			}

			Debug.LogError("Parsed chart has no notes");
			return null;
		}

		private static DeemoChart? ParseV3(string json)
		{
			var jobj = JObject.Parse(json);
			if (jobj["notes"] is not JArray notesArray)
			{
				Debug.LogError("V3 chart JSON has no 'notes' array");
				return null;
			}

			// V3 uses integer $id, convert to string for Newtonsoft reference handling
			foreach (var noteTok in notesArray)
			{
				var idTok = noteTok["$id"];
				if (idTok != null)
					noteTok["$id"] = idTok.ToString();
			}

			// V3 uses "ref" (integer) instead of "$ref" in links
			if (jobj["links"] is JArray linksArray)
			{
				foreach (var linkTok in linksArray)
				{
					var linkNotes = linkTok["notes"] as JArray;
					if (linkNotes == null) continue;
					foreach (var refTok in linkNotes)
					{
						var refVal = refTok["ref"];
						if (refVal != null)
							refTok["$ref"] = refVal.ToString();
					}
				}
			}

			var serializer = JsonSerializer.Create(new JsonSerializerSettings());
			var chart = jobj.ToObject<DeemoChart>(serializer);
			if (chart is not { Notes.Count: > 0 })
				Debug.LogError("V3 chart parsed but has no notes");
			return chart;
		}
	}
}
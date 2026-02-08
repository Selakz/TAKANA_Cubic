#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Easing;

namespace MusicGame.Utility.Dakumi
{
	public struct Beat : IEquatable<Beat>, IComparable<Beat>
	{
		public int BeatCount { get; set; }

		public int Numor { get; set; }

		public int Denom { get; set; }

		public static Beat Default => new Beat { BeatCount = 0, Numor = 0, Denom = 1 };

		public double GetTotalBeats() => BeatCount + (double)Numor / Denom;

		public int CompareTo(Beat other)
		{
			var beatCompare = BeatCount.CompareTo(other.BeatCount);
			return beatCompare != 0
				? beatCompare
				: ((float)Numor / Denom).CompareTo((float)other.Numor / other.Denom);
		}

		public bool Equals(Beat other) => BeatCount == other.BeatCount && Numor == other.Numor && Denom == other.Denom;

		public override bool Equals(object? obj) => obj is Beat other && Equals(other);

		public override int GetHashCode() => HashCode.Combine(BeatCount, Numor, Denom);

		public static Beat FromJToken(JToken? token)
		{
			if (token is not JArray { Count: 3 } array) return Default;
			return new Beat
			{
				BeatCount = array[0].Value<int>(),
				Numor = array[1].Value<int>(),
				Denom = array[2].Value<int>()
			};
		}
	}

	public enum NoteType
	{
		Note,
		Wipe,
		Hold
	}

	public class Note
	{
		public int Track { get; set; } = 0;

		public Beat StartBeat { get; set; } = Beat.Default;

		public Beat EndBeat { get; set; } = Beat.Default;

		public NoteType Type { get; set; } = NoteType.Note;

		public bool IsFake { get; set; } = false;

		public bool WithNoteHead { get; set; } = false;

		public bool WithWipeHead { get; set; } = false;

		public static Note FromJObject(JObject? dict)
		{
			if (dict is null) return new Note();
			return new Note
			{
				Track = dict.Get("track", 0),
				StartBeat = Beat.FromJToken(dict["beat"]),
				EndBeat = Beat.FromJToken(dict["beat2"]),
				Type = dict.Get("type", "note") switch
				{
					"note" => NoteType.Note,
					"wipe" => NoteType.Wipe,
					"hold" => NoteType.Hold,
					_ => NoteType.Note
				},
				IsFake = dict.Get("fake", 0) == 1,
				WithNoteHead = dict.Get("note_head", 0) == 1,
				WithWipeHead = dict.Get("wipe_head", 0) == 1
			};
		}
	}

	public class Transformation
	{
		public float[] BezierTrans { get; } = { 0, 0, 1, 1 };

		public Eases Easing { get; set; } = Eases.Unmove;

		public string Type { get; set; } = "bezier";

		public static Transformation FromJObject(JObject? dict)
		{
			if (dict is null) return new Transformation();
			var ret = new Transformation
			{
				Easing = CurveCalculator.GetEaseByRpeNumber(dict.Get("easings", 0)),
				Type = dict.Get("type", "bezier"),
			};
			if (dict["trans"] is JArray { Count: 4 } transArray)
			{
				for (int i = 0; i < transArray.Count; i++)
				{
					ret.BezierTrans[i] = transArray[i].Value<float>();
				}
			}

			return ret;
		}
	}

	public class Event
	{
		public int Track { get; set; } = 0;

		public Beat StartBeat { get; set; } = Beat.Default;

		public Beat EndBeat { get; set; } = Beat.Default;

		public int Type { get; set; } = 1; // Corresponding to movement1/2

		public float From { get; set; } = 0;

		public float To { get; set; } = 0;

		public Transformation Trans { get; set; } = new();

		public static Event FromJObject(JObject? dict)
		{
			if (dict is null) return new Event();
			return new Event
			{
				Track = dict.Get("track", 0),
				StartBeat = Beat.FromJToken(dict["beat"]),
				EndBeat = Beat.FromJToken(dict["beat2"]),
				Type = dict.Get("type", "x") == "x" ? 1 : 2,
				From = dict.Get("from", 0f),
				To = dict.Get("to", 0f),
				Trans = Transformation.FromJObject(dict["trans"] is JObject obj ? obj : null)
			};
		}
	}

	public class TrackInfo
	{
		public string Name { get; set; } = string.Empty;

		public bool ShowWhen0 { get; set; } = true;

		public string Type { get; set; } = "xw";

		public static TrackInfo FromJObject(JObject? dict)
		{
			if (dict is null) return new TrackInfo();
			return new TrackInfo
			{
				Name = dict.Get("name", string.Empty),
				ShowWhen0 = dict.Get("w0thenShow", 0) == 1,
				Type = dict.Get("type", "xw")
			};
		}
	}

	public class ChartMeta
	{
		public string Artist { get; set; } = string.Empty;

		public string SongName { get; set; } = string.Empty;

		public string ChartName { get; set; } = string.Empty;

		public string Charter { get; set; } = string.Empty;

		public static ChartMeta FromJObject(JObject? dict)
		{
			if (dict is null) return new ChartMeta();
			return new ChartMeta
			{
				Artist = dict.Get("artist", string.Empty),
				SongName = dict.Get("song_name", string.Empty),
				ChartName = dict.Get("chart_name", string.Empty),
				Charter = dict.Get("chartor", string.Empty)
			};
		}
	}

	public class Preference
	{
		public float XOffset { get; set; } = 0;

		public float EventScale { get; set; } = 100;

		public static Preference FromJObject(JObject? dict)
		{
			if (dict is null) return new Preference();
			return new Preference
			{
				XOffset = dict.Get("x_offset", 0f),
				EventScale = dict.Get("event_scale", 100f)
			};
		}
	}

	public class DakumiChart
	{
		public SortedDictionary<Beat, float> BpmList { get; set; } = new() { [Beat.Default] = 100 };

		public List<Note> Notes { get; set; } = new();

		public Dictionary<int, List<Event>> Events { get; set; } = new();

		public Dictionary<int, TrackInfo> Tracks { get; set; } = new();

		public T3Time Offset { get; set; } = 0;

		public ChartMeta Info { get; set; } = new();

		public Preference Preference { get; set; } = new();

		public static DakumiChart FromJObject(JObject? dict)
		{
			var chart = new DakumiChart();
			if (dict is null) return chart;
			SortedDictionary<Beat, float> bpmList = new() { [Beat.Default] = 100 };
			// BpmList
			if (dict["bpm_list"] is JArray bpmArray)
			{
				foreach (var item in bpmArray)
				{
					if (item is not JObject bpmItem) continue;
					bpmList[Beat.FromJToken(bpmItem["beat"])] = bpmItem.Get("bpm", 100f);
				}

				chart.BpmList = bpmList;
			}

			// Notes
			if (dict["note"] is JArray noteArray)
			{
				foreach (var note in noteArray)
				{
					if (note is not JObject noteItem) continue;
					chart.Notes.Add(Note.FromJObject(noteItem));
				}
			}

			// Events
			if (dict["event"] is JArray eventArray)
			{
				foreach (var token in eventArray)
				{
					if (token is not JObject eventItem) continue;
					var d3Event = Event.FromJObject(eventItem);
					chart.Events.TryAdd(d3Event.Track, new());
					chart.Events[d3Event.Track].Add(d3Event);
				}
			}

			foreach (var list in chart.Events.Values) list.Sort((a, b) => a.StartBeat.CompareTo(b.StartBeat));

			// Tracks
			if (dict["track"] is JObject trackArray)
			{
				foreach (var (key, track) in trackArray)
				{
					if (!int.TryParse(key, out var id) || track is not JObject trackItem) continue;
					chart.Tracks[id] = TrackInfo.FromJObject(trackItem);
				}
			}

			// Offset
			chart.Offset = dict.Get("offset", 0);
			// Info
			if (dict["info"] is JObject infoObject) chart.Info = ChartMeta.FromJObject(infoObject);
			// Preference
			if (dict["preference"] is JObject prefObject) chart.Preference = Preference.FromJObject(prefObject);

			return chart;
		}
	}
}
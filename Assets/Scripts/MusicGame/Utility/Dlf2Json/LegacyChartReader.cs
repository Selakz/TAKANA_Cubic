using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MusicGame.Components;
using MusicGame.Components.Chart;
using MusicGame.Components.JudgeLines;
using MusicGame.Components.Movement;
using MusicGame.Components.Notes;
using MusicGame.Components.Tracks;
using MusicGame.Components.Tracks.Movement;
using T3Framework.Runtime;
using T3Framework.Static.Easing;
using UnityEngine;

namespace MusicGame.Utility.Dlf2Json
{
	public class LegacyChartReader
	{
		// 用于初始化ChartInfo
		private float offset = 0f;
		private readonly List<IComponent> componentList = new();

		// 读谱过程的全局变量
		private string[] chart;
		private int ptr = 0; // 谱面行数指针
		private bool isOffsetDone = false;
		private bool isChartProcess = false;
		private Tap g_Tap;
		private Slide g_Slide;
		private Hold g_Hold;
		private Track g_Track;
		private JudgeLine g_Line;

		// Static
		// 看来有些表达式之后不太能写成常量的样子...
		private const string uInt = @"\d+",
			uFloat = @"\d+(?:\.\d+)?",
			Float = @"-?\d+(?:\.\d+)?";

		private const string Curve = @"(?:\S{1,2})";

		//private const string Color = @"(?:t|r|b|d)";
		private enum Regexes
		{
			Empty,
			Offset,
			Split,
			Custom,
			UTap,
			UTapS,
			UPos,
			Speed,
			Trail,
			Track,
			LPos,
			RPos,
			Tap,
			TapS,
			Slide,
			SlideS,
			Hold,
			HoldS,
			Scale,
			Camera,
			Plot,
			LayerName,
			LayerOrder
		}

		private readonly Dictionary<Regexes, string> regexes = new()
		{
			{ Regexes.Empty, @"^\s*$" },
			{ Regexes.Offset, @$"^\s*offset\s*\(\s*({uInt})\s*\);" },
			{ Regexes.Split, @"^\s*-+\s*$" },
			{ Regexes.Custom, @"^\s*(\S*)\s*:\s*(\S*)\s*;" },
			{ Regexes.UTap, @$"^\s*upress\s*\(\s*({uInt}),\s*({uFloat}),\s*({Float})\s*\)\s*;" },
			{ Regexes.UTapS, @$"^\s*upress\s*\(\s*({uInt}),\s*({uFloat}),\s*({Float}),\s*({Float})\s*\)\s*;" },
			{
				Regexes.UPos,
				@$"^\s*pos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"
			},
			{ Regexes.Speed, @$"^\s*speed\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;" },
			{ Regexes.Trail, @$"^\s*trail\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;" },

			// 理论上原游戏只需要用到以下的表达式
			{ Regexes.Track, @$"^\s*track\s*\(\s*({uInt}),\s*({uInt}),\s*({Float}),\s*({Float})\s*\)\s*;" },
			{
				Regexes.LPos,
				@$"^\s*lpos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"
			},
			{
				Regexes.RPos,
				@$"^\s*rpos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"
			},
			{ Regexes.Tap, @$"^\s*tap\s*\(\s*({uInt})\s*\)\s*;" },
			{ Regexes.TapS, @$"^\s*tap\s*\(\s*({uInt}),\s*({Float})\s*\)\s*;" },
			{ Regexes.Slide, @$"^\s*slide\s*\(\s*({uInt})\s*\)\s*;" },
			{ Regexes.SlideS, @$"^\s*slide\s*\(\s*({uInt}),\s*({Float})\s*\)\s*;" },
			{ Regexes.Hold, @$"^\s*hold\s*\(\s*({uInt}),\s*({uInt})\s*\)\s*;" },
			{ Regexes.HoldS, @$"^\s*hold\s*\(\s*({uInt}),\s*({uInt}),\s*({Float})\s*\)\s*;" },
			{ Regexes.LayerName, @$"^\s*layername\s*\(((?:\s*{uInt},\s*\S*,)*(?:\s*{uInt},\s*\S*))\s*\)\s*;" },
			{ Regexes.LayerOrder, @$"^\s*layerorder\s*\(((?:\s*{uInt},)*(?:\s*{uInt}))\s*\)\s*;" },

			{ Regexes.Scale, @$"^\s*scale\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;" },
			{ Regexes.Camera, @$"camera" },
			{ Regexes.Plot, @$"plot" },
		};

		private readonly HashSet<Regexes> BaseRegex = new()
		{
			Regexes.Empty, Regexes.Offset, Regexes.Split, Regexes.Custom, Regexes.LayerName, Regexes.LayerOrder,
			Regexes.UTap, Regexes.UTapS, Regexes.Track, Regexes.Camera, Regexes.Plot,
		};

		private readonly HashSet<Regexes> UNoteRegex = new()
		{
			Regexes.UPos, Regexes.Speed, Regexes.Trail, Regexes.Empty,
		};

		private readonly HashSet<Regexes> TrackRegex = new()
		{
			Regexes.LPos, Regexes.Tap, Regexes.TapS, Regexes.Slide, Regexes.SlideS, Regexes.Hold, Regexes.HoldS,
			Regexes.Empty
		};

		private readonly HashSet<Regexes> TNoteRegex = new()
		{
			Regexes.Speed, Regexes.Trail, Regexes.Empty
		};

		private readonly List<Regexes> THoldRegex = new()
		{
			Regexes.Speed, Regexes.Trail, Regexes.Scale, Regexes.Empty
		};

		// Defined Function
		private LegacyChartReader()
		{
		}

		public static ChartInfo Read(string content)
		{
			ChartInfo chartInfo = new LegacyChartReader().InClassRead(content);
			return chartInfo;
		}

		private ChartInfo InClassRead(string rawChart)
		{
			chart = rawChart.Split('\n');
			DeleteAnnotation(chart);

			IComponent.ResetId();
			g_Line = new();
			componentList.Add(g_Line);

			for (ptr = 0; ptr < chart.Length; ptr++)
			{
				bool isValidLine = false;
				foreach (var pair in regexes)
				{
					var match = Regex.Match(chart[ptr], pair.Value);
					if (match.Success && BaseRegex.Contains(pair.Key))
					{
						Action<Match> action;
						if (!isChartProcess)
						{
							action = pair.Key switch
							{
								Regexes.Empty => CaseDoNothing,
								Regexes.Offset => CaseOffset,
								Regexes.Split => CaseSplit,
								Regexes.Custom => CaseCustom,
								Regexes.LayerName => CaseLayerName,
								Regexes.LayerOrder => CaseLayerOrder,
								_ => AssertError
							};
						}
						else
						{
							action = pair.Key switch
							{
								Regexes.Empty => CaseDoNothing,
								Regexes.UTap => CaseUTap,
								Regexes.UTapS => CaseUTap,
								Regexes.Track => CaseTrack,
								Regexes.Camera => CaseCamera,
								Regexes.Plot => CasePlot,
								_ => AssertError
							};
						}

						action.Invoke(match);
						isValidLine = true;
						break;
					}
				}

				if (!isValidLine) AssertError("Find invalid content.");
			}

			return new(offset, componentList);
		}

		public static void DeleteAnnotation(string[] chart)
		{
			for (int i = 0; i < chart.Length; i++)
			{
				int annoPos = chart[i].IndexOf("//", StringComparison.Ordinal);
				if (annoPos >= 0) chart[i] = chart[i].Remove(annoPos);
			}
		}

		private void CaseDoNothing(Match match)
		{
		}

		private void CaseOffset(Match match)
		{
			// @$"^\s*offset\s*\(\s*({uInt})\s*\);"
			if (!isOffsetDone && !isChartProcess)
			{
				offset = int.Parse(match.Groups[1].Value) / 1000f;
				isOffsetDone = true;
			}
			else
			{
				AssertError("Wrong offset position at line: ");
			}
		}

		private void CaseSplit(Match match)
		{
			// @"^\s*-+\s*$"
			if (!isChartProcess)
			{
				// 虽然传入参数没什么用但是为了方便委托还是加了...
				isChartProcess = true;
			}
			else
			{
				AssertError("Duplicate split line in chart.");
			}
		}

		private void CaseCustom(Match match)
		{
			// @"^\s*(\S*)\s*:\s*(\S*)\s*;"
		}

		private void CaseUTap(Match match)
		{
		}

		private void CaseUPos(Match match, float width, float posEnd)
		{
			// @$"^\s*pos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"
			string[] posList = match.Groups[1].Value.Split(',');
			List<(float time, float x, string curve)> lPosList = new(), rPosList = new();

			for (int i = 0; i < posList.Length; i += 3)
			{
				// (time, pos, curve)
				float time = int.Parse(posList[i]) / 1000f;
				float pos = float.Parse(posList[i + 1]);
				string curve = posList[i + 2].Trim();
				if (lPosList.Count != 0 && time < lPosList[^1].time)
					AssertError("The time in pos line is not in nondecreasing order.");
				if (time < g_Track.TimeInstantiate || time > g_Track.TimeEnd)
					AssertError("The time in pos line is out of range.");
				lPosList.Add((time, pos - width / 2, curve));
				rPosList.Add((time, pos + width / 2, curve));
			}

			// g_Track.LMoveList = new(lPosList, g_Track.TimeInstantiate, g_Track.TimeEnd, posEnd - width / 2);
			// g_Track.RMoveList = new(rPosList, g_Track.TimeInstantiate, g_Track.TimeEnd, posEnd + width / 2);
		}

		private void CaseSpeed(Match match)
		{
			// @$"^\s*speed\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;
		}

		private void CaseTrail(Match match)
		{
			// @$"^\s*trail\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;
		}

		private void CaseTrack(Match match)
		{
			T3Time timeStart = int.Parse(match.Groups[1].Value);
			T3Time timeEnd = int.Parse(match.Groups[2].Value);
			float lPosEnd = float.Parse(match.Groups[3].Value);
			float rPosEnd = float.Parse(match.Groups[4].Value);

			// 构造一个完整的track
			g_Track = new(timeStart, timeEnd, g_Line);
			componentList.Add(g_Track);

			// 处理该track的所有修饰行
			bool isNewElement = false, hasPosLine = false;
			while (!isNewElement && ptr + 1 < chart.Length)
			{
				bool isValidLine = false;
				foreach (var pair in regexes)
				{
					var nextMatch = Regex.Match(chart[ptr + 1], pair.Value);
					if (nextMatch.Success)
					{
						isValidLine = true;
						if (TrackRegex.Contains(pair.Key))
						{
							ptr++;
							switch (pair.Key)
							{
								case Regexes.LPos:
									if (!hasPosLine)
									{
										hasPosLine = true;
										CaseLPos(nextMatch, lPosEnd, rPosEnd);
									}
									else AssertError("Duplicate position property.");

									break;
								case Regexes.Tap:
								case Regexes.TapS:
									CaseTap(nextMatch);
									break;
								case Regexes.Hold:
								case Regexes.HoldS:
									CaseHold(nextMatch);
									break;
								case Regexes.Slide:
								case Regexes.SlideS:
									CaseSlide(nextMatch);
									break;
							}

							break;
						}
						else if (BaseRegex.Contains(pair.Key))
						{
							isValidLine = true;
							isNewElement = true;
							break;
						}

						else AssertError("Property appears in a wrong place.");
					}
				}

				if (!isValidLine)
				{
					ptr++;
					AssertError("Find invalid content.");
				}
			}
		}

		private void CaseLPos(Match match, float lPosEnd, float rPosEnd)
		{
			// @$"^\s*lpos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"
			// @$"^\s*rpos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"
			// 这是一个双行修饰行，因此没有CaseRPos这个方法。
			string[] lPosLine = match.Groups[1].Value.Split(',');
			ptr++;
			Match rPosMatch = Regex.Match(chart[ptr], regexes[Regexes.RPos]);
			if (!rPosMatch.Success)
			{
				AssertError("Find lpos line not following rpos line.");
				return;
			}

			string[] rPosLine = Regex.Match(chart[ptr], regexes[Regexes.RPos]).Groups[1].Value.Split(',');

			V1EMoveList lPosList = new();
			for (int i = 0; i < lPosLine.Length; i += 3)
			{
				// (time, pos, curve)
				T3Time time = int.Parse(lPosLine[i]);
				float x = float.Parse(lPosLine[i + 1]);
				string curve = lPosLine[i + 2].Trim();
				Eases ease = CurveCalculator.GetEaseByName(curve);
				if (lPosList.Count != 0 && time < lPosList[^1].Time)
					AssertError("The time in pos line is not in nondecreasing order.");
				if (time < g_Track.TimeInstantiate || time > g_Track.TimeEnd)
					AssertError($"The time in pos line is out of range: {time}");
				lPosList.Insert(new(time, x, ease));
			}

			lPosList.Insert(new V1EMoveItem(g_Track.TimeEnd, lPosEnd, Eases.Unmove));

			V1EMoveList rPosList = new();
			for (int i = 0; i < rPosLine.Length; i += 3)
			{
				// (time, pos, curve)
				T3Time time = int.Parse(rPosLine[i]);
				float x = float.Parse(rPosLine[i + 1]);
				string curve = rPosLine[i + 2].Trim();
				Eases ease = CurveCalculator.GetEaseByName(curve);
				if (rPosList.Count != 0 && time < rPosList[^1].Time)
					AssertError("The time in pos line is not in nondecreasing order.");
				if (time < g_Track.TimeInstantiate || time > g_Track.TimeEnd)
					AssertError($"The time in pos line is out of range: {time}");
				rPosList.Insert(new(time, x, ease));
			}

			rPosList.Insert(new V1EMoveItem(g_Track.TimeEnd, rPosEnd, Eases.Unmove));

			g_Track.Movement = new TrackEdgeMovement(lPosList, (IMovement<float>)rPosList);
		}

		private void CaseTap(Match match)
		{
			T3Time timeJudge = int.Parse(match.Groups[1].Value);
			if (timeJudge < g_Track.TimeInstantiate || timeJudge > g_Track.TimeEnd)
			{
				AssertError("Tnote's judge time is out of range of its track.");
				return;
			}

			// 构造一个完整的tap
			g_Tap = new(timeJudge, g_Track);
			componentList.Add(g_Tap);

			// 处理该tap的所有修饰行
			bool isNewElement = false, hasSpeedLine = false;
			while (!isNewElement && ptr + 1 < chart.Length)
			{
				bool isValidLine = false;
				foreach (var pair in regexes)
				{
					var nextMatch = Regex.Match(chart[ptr + 1], pair.Value);
					if (nextMatch.Success)
					{
						isValidLine = true;
						if (TNoteRegex.Contains(pair.Key))
						{
							ptr++;
							switch (pair.Key)
							{
								case Regexes.Speed:
									if (!hasSpeedLine)
									{
										hasSpeedLine = true;
										CaseSpeed(nextMatch);
									}
									else AssertError("Duplicate movement property.");

									break;
								case Regexes.Trail:
									if (!hasSpeedLine)
									{
										hasSpeedLine = true;
										CaseTrail(nextMatch);
									}
									else AssertError("Duplicate movement property.");

									break;
							}

							break;
						}
						else if (BaseRegex.Contains(pair.Key) || TrackRegex.Contains(pair.Key))
						{
							isValidLine = true;
							isNewElement = true;
							break;
						}
						else AssertError("Property appears in a wrong place.");
					}
				}

				if (!isValidLine)
				{
					ptr++;
					AssertError("Find invalid content.");
				}
			}
		}

		private void CaseSlide(Match match)
		{
			T3Time timeJudge = int.Parse(match.Groups[1].Value);
			if (timeJudge < g_Track.TimeInstantiate || timeJudge > g_Track.TimeEnd)
			{
				AssertError("Tnote's judge time is out of range of its track.");
				return;
			}

			// 构造一个完整的slide
			g_Slide = new(timeJudge, g_Track);
			componentList.Add(g_Slide);

			// 处理该slide的所有修饰行
			bool isNewElement = false, hasSpeedLine = false;
			while (!isNewElement && ptr + 1 < chart.Length)
			{
				bool isValidLine = false;
				foreach (var pair in regexes)
				{
					var nextMatch = Regex.Match(chart[ptr + 1], pair.Value);
					if (nextMatch.Success)
					{
						isValidLine = true;
						if (TNoteRegex.Contains(pair.Key))
						{
							ptr++;
							switch (pair.Key)
							{
								case Regexes.Speed:
									if (!hasSpeedLine)
									{
										hasSpeedLine = true;
										CaseSpeed(nextMatch);
									}
									else AssertError("Duplicate movement property.");

									break;
								case Regexes.Trail:
									if (!hasSpeedLine)
									{
										hasSpeedLine = true;
										CaseTrail(nextMatch);
									}
									else AssertError("Duplicate movement property.");

									break;
							}

							break;
						}
						else if (BaseRegex.Contains(pair.Key) || TrackRegex.Contains(pair.Key))
						{
							isValidLine = true;
							isNewElement = true;
							break;
						}
						else AssertError("Property appears in a wrong place.");
					}
				}

				if (!isValidLine)
				{
					ptr++;
					AssertError("Find invalid content.");
				}
			}
		}

		private void CaseHold(Match match)
		{
			T3Time timeJudge = int.Parse(match.Groups[1].Value);
			T3Time timeEnd = int.Parse(match.Groups[2].Value);
			if (timeJudge < g_Track.TimeInstantiate || timeJudge > g_Track.TimeEnd)
			{
				AssertError("Thold's judge time is out of range of its track.");
				return;
			}

			if (timeEnd < g_Track.TimeInstantiate || timeEnd > g_Track.TimeEnd)
			{
				AssertError("Thold's end time is out of range of its track.");
				return;
			}

			if (timeJudge >= timeEnd)
			{
				AssertError("Thold's end time is earlier than its judge time.");
				return;
			}

			// 构造一个完整的hold
			g_Hold = new(timeJudge, timeEnd, g_Track);
			componentList.Add(g_Hold);

			// 处理该hold的所有修饰行
			bool isNewElement = false, hasSpeedLine = false, hasScaleLine = false;
			while (!isNewElement && ptr + 1 < chart.Length)
			{
				bool isValidLine = false;
				foreach (var pair in regexes)
				{
					var nextMatch = Regex.Match(chart[ptr + 1], pair.Value);
					if (nextMatch.Success)
					{
						isValidLine = true;
						if (THoldRegex.Contains(pair.Key))
						{
							ptr++;
							switch (pair.Key)
							{
								case Regexes.Speed:
									if (!hasSpeedLine)
									{
										hasSpeedLine = true;
										CaseHoldSpeed(nextMatch);
									}
									else AssertError("Duplicate movement property.");

									break;
								case Regexes.Trail:
									if (!hasSpeedLine)
									{
										hasSpeedLine = true;
										CaseHoldTrail(nextMatch);
									}
									else AssertError("Duplicate movement property.");

									break;
								case Regexes.Scale:
									if (!hasScaleLine)
									{
										hasScaleLine = true;
										CaseScale(nextMatch);
									}
									else AssertError("Duplicate scale property.");

									break;
							}

							break;
						}
						else if (BaseRegex.Contains(pair.Key) || TrackRegex.Contains(pair.Key))
						{
							isValidLine = true;
							isNewElement = true;
							break;
						}
						else AssertError("Property appears in a wrong place.");
					}
				}

				if (!isValidLine)
				{
					ptr++;
					AssertError("Find invalid content.");
				}
			}
		}

		private void CaseHoldSpeed(Match match)
		{
			// @$"^\s*speed\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;
		}

		private void CaseHoldTrail(Match match)
		{
			// @$"^\s*trail\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;
		}

		private void CaseScale(Match match)
		{
			// @$"^\s*scale\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;
		}

		private void CaseCamera(Match match)
		{
			// Camera line read when finished relative functions.
		}

		private void CasePlot(Match match)
		{
			// Plot line read when finished relative functions.
		}

		private void CaseLayerName(Match match)
		{
		}

		private void CaseLayerOrder(Match match)
		{
		}

		private void AssertError(string message)
		{
			throw new Exception($"{message} Position: Line {ptr + 1}");
		}

		private void AssertError(Match match)
		{
			throw new Exception($"Property is at a wrong place. Position: Line {ptr + 1}");
		}
	}
}
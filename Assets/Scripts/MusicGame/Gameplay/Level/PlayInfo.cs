#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	public class PlayData
	{
		public int Score { get; set; }
	}

	public class PlayInfo : ISingletonSetting<PlayInfo>
	{
		public Dictionary<string, Dictionary<int, PlayData>> Data { get; set; } = new();

		public PlayData? GetPlayData(string songId, int difficulty)
		{
			return string.IsNullOrEmpty(songId)
				? null
				: Data.TryGetValue(songId, out var dict) && dict.TryGetValue(difficulty, out var data)
					? data
					: null;
		}

		public void AddPlayData(string songId, int difficulty, PlayData playData)
		{
			if (string.IsNullOrEmpty(songId)) return;
			Data.TryAdd(songId, new Dictionary<int, PlayData>());
			Data[songId][difficulty] = playData;
		}

		public void SetHighScore(string songId, int difficulty, int score)
		{
			if (GetPlayData(songId, difficulty) is { } playData)
			{
				playData.Score = Mathf.Max(playData.Score, score);
			}
			else
			{
				AddPlayData(songId, difficulty, new PlayData { Score = score });
			}
		}
	}
}
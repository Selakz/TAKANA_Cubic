#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	/// <summary> Difficulty id rules and metadata resolution shared by editor and level select. </summary>
	public static class DifficultyCatalog
	{
		public const int BuiltInMin = 1;
		public const int BuiltInMax = 5;
		public const int CustomIdStart = 11;

		public static bool IsBuiltIn(int id) => id is >= BuiltInMin and <= BuiltInMax;

		/// <summary> Whether the given difficulty is a custom one declared by the song. </summary>
		public static bool IsCustom(SongInfo? songInfo, int id) =>
			songInfo is not null &&
			songInfo.Difficulties.TryGetValue(id, out var info) && info.IsCustom;

		/// <summary> A built-in difficulty, or a custom difficulty whose id is not below <see cref="CustomIdStart"/>. </summary>
		public static bool IsValid(int id) => IsBuiltIn(id) || id >= CustomIdStart;

		public static List<int> GetCustomIds(SongInfo? songInfo)
		{
			if (songInfo is null) return new();
			return songInfo.Difficulties
				.Where(pair => pair.Value.IsCustom)
				.Select(pair => pair.Key)
				.OrderBy(id => id)
				.ToList();
		}

		public static int NextCustomId(SongInfo? songInfo)
		{
			var max = CustomIdStart - 1;
			foreach (var id in GetCustomIds(songInfo))
			{
				if (id > max) max = id;
			}

			return max + 1;
		}

		/// <summary> Custom difficulties carry their own name/color; built-in ones come from the config asset. </summary>
		public static (string name, Color color, bool found) ResolveNameColor(
			SongInfo? songInfo, int id, DifficultyConfig? config)
		{
			if (songInfo is not null &&
			    songInfo.Difficulties.TryGetValue(id, out var info) && info.IsCustom)
			{
				return (info.Name, info.Color, true);
			}

			if (config is not null && config.Value.TryGetValue(id, out var data))
			{
				return (data.name, data.color, true);
			}

			return (string.Empty, Color.white, false);
		}
	}
}

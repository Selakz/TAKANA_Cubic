#nullable enable

using MusicGame.Gameplay.Chart;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	public class LevelInfo
	{
		/// <summary> The path of .t3proj </summary>
		public string LevelPath { get; set; } = string.Empty;

		public int Difficulty { get; set; } = 3;

		public Texture2D? Cover { get; set; }

		public AudioClip? Music { get; set; }

		public ChartInfo Chart { get; set; } = new();

		public SongInfo SongInfo { get; set; } = new();

		public IPreference? Preference { get; set; }
	}
}
#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models.Note;
using MusicGame.Models.Track;

namespace EditorPlugin.Tests
{
	/// <summary> State handed to the C# side checks of one scenario (see <see cref="PluginSystemChecks" />). </summary>
	public sealed class PluginTestCheckContext
	{
		private readonly PluginTestReport report;

		public string Scenario { get; }

		/// <summary> The level the editor runs on; its chart is the chart the plugin context of the scenario uses. </summary>
		public LevelInfo LevelInfo { get; }

		public ChartInfo Chart => LevelInfo.Chart;

		/// <summary> What the editor's command manager did while the scenario ran. </summary>
		public PluginTestCommandRecorder Commands { get; }

		public IEnumerable<ChartComponent> Tracks => Chart.Where(component => component.Model is ITrack);

		public IEnumerable<ChartComponent> Notes => Chart.Where(component => component.Model is INote);

		public PluginTestCheckContext(string scenario, LevelInfo levelInfo, PluginTestCommandRecorder commands,
			PluginTestReport report)
		{
			Scenario = scenario;
			LevelInfo = levelInfo;
			Commands = commands;
			this.report = report;
		}

		public void Check(bool ok, string name, string? detail = null) => report.Check(ok, name, detail);

		public void Fail(string name, string? detail = null) => report.Fail(name, detail);

		public void Log(string message) => report.Log(message);
	}
}

#nullable enable

using System;

namespace EditorPlugin.Tests
{
	/// <summary>
	/// Object handed to the test plugin's <c>runTests(harness)</c> entry. It is marshalled into the plugin's
	/// JavaScript environment by PuerTS, so every method must keep all of its parameters required.
	/// </summary>
	public sealed class PluginTestHarness
	{
		private readonly PluginTestRunner runner;

		public PluginTestHarness(PluginTestRunner runner)
		{
			this.runner = runner;
		}

		/// <summary> Runs one scenario on a fresh chart of the test level; the C# side checks run right after the body. </summary>
		public void scenario(string name, Action body) => runner.RunScenario(name, body);

		public void check(bool ok, string name, string detail) => runner.Report.Check(ok, name, detail);

		public void fail(string name, string detail) => runner.Report.Fail(name, detail);

		public void log(string message) => runner.Report.Log(message);

		/// <summary> Undo the last commit of the current scenario. </summary>
		public void undo() => runner.Undo();

		/// <summary> Redo the last undone commit of the current scenario. </summary>
		public void redo() => runner.Redo();
	}
}

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorPlugin.Tests
{
	public enum PluginTestAssertionKind
	{
		Pass,
		Fail,
		Error,
		Log
	}

	/// <summary> A check, error or log produced while running the plugin system tests. </summary>
	public sealed class PluginTestAssertion
	{
		public string Scenario { get; }

		public string Name { get; }

		public PluginTestAssertionKind Kind { get; }

		public string? Detail { get; }

		public PluginTestAssertion(string scenario, string name, PluginTestAssertionKind kind, string? detail)
		{
			Scenario = scenario;
			Name = name;
			Kind = kind;
			Detail = detail;
		}
	}

	/// <summary> Result of one plugin system test run. </summary>
	public sealed class PluginTestReport
	{
		/// <summary> Scenario name used for assertions that are not part of a scenario (setup, compile, run). </summary>
		public const string RunScenarioName = "run";
		private readonly List<PluginTestAssertion> assertions = new();
		private string scenario = RunScenarioName;

		public IReadOnlyList<PluginTestAssertion> Assertions => assertions;

		public int PassCount { get; private set; }

		public int FailCount { get; private set; }

		public int ErrorCount { get; private set; }

		public int ScenarioCount { get; private set; }

		public string? WorkspaceDirectory { get; set; }

		public TimeSpan Duration { get; set; }

		public bool Succeeded => FailCount == 0 && ErrorCount == 0 && ScenarioCount > 0;

		public void BeginScenario(string name)
		{
			scenario = name;
			ScenarioCount++;
		}

		public void EndScenario() => scenario = RunScenarioName;

		public void Check(bool ok, string name, string? detail = null)
		{
			if (ok)
			{
				PassCount++;
				assertions.Add(new(scenario, name, PluginTestAssertionKind.Pass, detail));
				return;
			}

			Fail(name, detail);
		}

		public void Fail(string name, string? detail = null)
		{
			FailCount++;
			assertions.Add(new(scenario, name, PluginTestAssertionKind.Fail, detail));
		}

		public void Error(string name, string? detail = null)
		{
			ErrorCount++;
			assertions.Add(new(scenario, name, PluginTestAssertionKind.Error, detail));
		}

		public void Log(string message) =>
			assertions.Add(new(scenario, string.Empty, PluginTestAssertionKind.Log, message));

		/// <summary> Human readable summary, meant to be printed with Debug.Log / Debug.LogError. </summary>
		public string Format()
		{
			StringBuilder builder = new();
			builder.AppendLine($"[PluginTest] {PassCount} passed, {FailCount} failed, {ErrorCount} errors " +
			                   $"({ScenarioCount} scenarios, {Duration.TotalSeconds:F1}s)");
			if (WorkspaceDirectory is not null) builder.AppendLine($"[PluginTest] workspace: {WorkspaceDirectory}");

			foreach (var group in assertions.GroupBy(assertion => assertion.Scenario))
			{
				if (group.Key.Length == 0) continue;
				var failures = group
					.Where(assertion => assertion.Kind is PluginTestAssertionKind.Fail or PluginTestAssertionKind.Error)
					.ToList();
				int passes = group.Count(assertion => assertion.Kind == PluginTestAssertionKind.Pass);
				if (failures.Count == 0)
				{
					if (passes > 0) builder.AppendLine($"[PluginTest] PASS {group.Key} ({passes} checks)");
					continue;
				}

				builder.AppendLine($"[PluginTest] SCENARIO {group.Key}: {passes} passed, " +
				                   $"{failures.Count(a => a.Kind == PluginTestAssertionKind.Fail)} failed, " +
				                   $"{failures.Count(a => a.Kind == PluginTestAssertionKind.Error)} errors");
				foreach (var failure in failures)
				{
					builder.AppendLine($"[PluginTest]   {failure.Kind.ToString().ToUpperInvariant()} {failure.Name}" +
					                   (string.IsNullOrEmpty(failure.Detail) ? string.Empty : $" -- {failure.Detail}"));
				}
			}

			foreach (var log in assertions.Where(assertion => assertion.Kind == PluginTestAssertionKind.Log))
			{
				builder.AppendLine($"[PluginTest] log {log.Scenario}: {log.Detail}");
			}

			return builder.ToString();
		}
	}
}

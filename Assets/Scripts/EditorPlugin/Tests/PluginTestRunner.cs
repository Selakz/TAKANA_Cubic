#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EditorPlugin.EditorIntegration;
using EditorPlugin.PluginSystem;
using EditorPlugin.PuerTS;
using EditorPlugin.Shared;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.CopyPaste;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using Puerts;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Log;
using T3Framework.Static.Event;
using UnityEngine;

namespace EditorPlugin.Tests
{
	/// <summary>
	/// Compiles the test plugin (see <see cref="PluginSystemPaths.TestPluginSourceDirectory" />), assigns a synthetic
	/// level to the editor and runs the scenarios the plugin declares through <see cref="PluginTestHarness" />.
	/// Everything runs on the scene's own systems: the plugin talks to the level's chart through the production
	/// bridge, commits into the editor's command manager and reaches the real datasets and views. Editor state is not
	/// restored afterwards, so a run is meant to end the current play session.
	/// </summary>
	public sealed class PluginTestRunner
	{
		private const string TestPluginEntry = "main.ts";
		private const string TestPluginJsEntry = "dist/main.js";
		private const int TestDifficulty = 3;
		private const int SilenceFrequency = 44100;
		private const int SilenceSampleCount = 44100;

		// Private
		private readonly NotifiableProperty<LevelInfo?> levelInfo;
		private readonly CommandManager commandManager;
		private readonly IDataset<ClipboardItem> clipboard;
		private readonly IBridgeBootstrapService bridgeService;
		private readonly PluginManageService manageService;
		private readonly string apiVersion;
		private readonly string sourceDirectory;
		private readonly string workspaceDirectory;
		private readonly PluginTestReport report = new();
		private readonly PluginTestCommandRecorder commands;
		private readonly List<string> executedScenarios = new();
		private readonly List<string> pluginErrorLogs = new();
		private readonly List<string> unityErrorLogs = new();
		private readonly Application.LogCallback unityLogHandler;
		private readonly Action<string, Enum> pluginLogHandler;

		private PluginRuntimeEnv? env;
		private PluginInstance? instance;
		private T3CSharpApi? currentApi;
		private PluginTestCheckContext? currentContext;

		public PluginTestReport Report => report;

		public PluginTestRunner(NotifiableProperty<LevelInfo?> levelInfo, CommandManager commandManager,
			IDataset<ClipboardItem> clipboard, IBridgeBootstrapService bridgeService,
			PluginManageService manageService, string? apiVersion) : this(
			levelInfo, commandManager, clipboard, bridgeService, manageService, apiVersion,
			PluginSystemPaths.TestPluginSourceDirectory, PluginSystemPaths.TestWorkspaceDirectory)
		{
		}

		public PluginTestRunner(NotifiableProperty<LevelInfo?> levelInfo, CommandManager commandManager,
			IDataset<ClipboardItem> clipboard, IBridgeBootstrapService bridgeService,
			PluginManageService manageService, string? apiVersion,
			string sourceDirectory, string workspaceDirectory)
		{
			this.levelInfo = levelInfo;
			this.commandManager = commandManager;
			this.clipboard = clipboard;
			this.bridgeService = bridgeService;
			this.manageService = manageService;
			this.apiVersion = string.IsNullOrEmpty(apiVersion) ? "0.1.0" : apiVersion!;
			this.sourceDirectory = sourceDirectory;
			this.workspaceDirectory = workspaceDirectory;
			commands = new PluginTestCommandRecorder(commandManager);
			unityLogHandler = OnUnityLog;
			pluginLogHandler = OnPluginLog;
		}

		/// <summary> Runs the whole suite synchronously and returns the report. </summary>
		public PluginTestReport Run()
		{
			var stopwatch = Stopwatch.StartNew();
			report.WorkspaceDirectory = workspaceDirectory;
			Application.logMessageReceived += unityLogHandler;
			T3Logger.AddListener("Notice", pluginLogHandler);
			try
			{
				RunInternal();
			}
			catch (Exception e)
			{
				report.Error("run", $"{e.GetType().Name}: {e.Message}\n{e.StackTrace}");
			}
			finally
			{
				Application.logMessageReceived -= unityLogHandler;
				T3Logger.RemoveListener("Notice", pluginLogHandler);
				DisposeRuntime();
			}

			report.Duration = stopwatch.Elapsed;
			foreach (string message in pluginErrorLogs) report.Error("run", $"plugin internal error: {message}");
			foreach (string message in unityErrorLogs) report.Error("run", $"error logged while running: {message}");
			foreach (string message in PluginSystemChecks.Errors) report.Error("run", $"invalid C# check: {message}");
			if (report.ScenarioCount > 0)
			{
				// A check whose scenario name does not match any scenario of the test plugin would silently never run.
				foreach (string scenario in PluginSystemChecks.ByScenario.Keys)
				{
					if (executedScenarios.Contains(scenario)) continue;
					report.Error("run", $"the C# check of scenario {scenario} never ran");
				}
			}

			if (report.ScenarioCount == 0) report.Error("run", "no scenario was executed");
			return report;
		}

		/// <summary> Runs one scenario; used by <see cref="PluginTestHarness.scenario" />. </summary>
		public void RunScenario(string name, Action body)
		{
			if (env is null)
			{
				report.Error(name, "the test plugin is not loaded");
				return;
			}

			report.BeginScenario(name);
			executedScenarios.Add(name);

			// The api of the previous scenario is dropped first: its node api still listens to the scene datasets and
			// would not be able to resolve nodes of the chart that is assigned next.
			currentApi?.Dispose();
			currentApi = null;
			if (!ResetChart(name))
			{
				FinishScenario();
				return;
			}

			currentApi = bridgeService.Initialize(env);
			if (currentApi is null)
			{
				report.Error(name, "the bridge did not initialize because the test level is gone");
				FinishScenario();
				return;
			}

			commands.Reset();
			currentContext = new PluginTestCheckContext(name, levelInfo.Value!, commands, clipboard, report);
			try
			{
				body.Invoke();
			}
			catch (Exception e)
			{
				report.Error(name, $"scenario threw {e.GetType().Name}: {e.Message}");
			}

			if (PluginSystemChecks.ByScenario.TryGetValue(name, out var check))
			{
				try
				{
					check.Invoke(currentContext);
				}
				catch (Exception e)
				{
					report.Error(name, $"check threw {e.GetType().Name}: {e.Message}");
				}
			}

			FinishScenario();

			void FinishScenario()
			{
				currentContext = null;
				report.EndScenario();
			}
		}

		public void Undo() => commandManager.Undo();

		public void Redo() => commandManager.Redo();

		private void RunInternal()
		{
			if (!Directory.Exists(sourceDirectory))
			{
				report.Error("setup", $"test plugin sources not found: {sourceDirectory}");
				return;
			}

			// Keeping the panel out of the run stops it from reloading the installed plugins onto the test level and
			// from ticking them while the scenarios run.
			manageService.enabled = false;
			report.Log("the editor plugin panel was disabled for this run");

			if (!AssignTestLevel()) return;
			SyncSources();
			if (!EnsureCompiled()) return;
			if (!LoadPlugin()) return;
			RunScenarios();
		}

		/// <summary>
		/// Assigns a synthetic level to the editor, so that the bridge and every level driven system work on a chart
		/// the tests own. The level points into the workspace, where no project file exists, so the auto save plugin
		/// never writes to a real project.
		/// </summary>
		private bool AssignTestLevel()
		{
			try
			{
				levelInfo.Value = new LevelInfo
				{
					LevelPath = Path.Combine(workspaceDirectory, "PluginTestLevel.t3proj"),
					Chart = new ChartInfo(),
					Music = AudioClip.Create("PluginTestSilence", SilenceSampleCount, 1, SilenceFrequency, false),
					Cover = null,
					SongInfo = new SongInfo
					{
						Title = { [Language.English] = "Plugin System Tests" },
						Composer = { [Language.English] = "Plugin System Tests" },
						Illustrator = { [Language.English] = "Plugin System Tests" },
						BpmDisplay = "120",
						Difficulties =
						{
							[TestDifficulty] = new DifficultyInfo
							{
								Charter = { [Language.English] = "Plugin System Tests" }
							}
						}
					},
					Preference = new EditorPreference(),
					Difficulty = TestDifficulty
				};
				return true;
			}
			catch (Exception e)
			{
				report.Error("setup", $"failed to assign the test level: {e.GetType().Name}: {e.Message}");
				return false;
			}
		}

		/// <summary>
		/// Swaps in a fresh chart. Every level driven system reacts to the notification: the command history and the
		/// selection are cleared, the layers are rebuilt and the views are generated for the new chart.
		/// </summary>
		private bool ResetChart(string scenario)
		{
			if (levelInfo.Value is not { } info)
			{
				report.Error(scenario, "the test level disappeared");
				return false;
			}

			try
			{
				info.Chart = new ChartInfo();
				levelInfo.ForceNotify();
				return true;
			}
			catch (Exception e)
			{
				report.Error(scenario, $"failed to reset the chart: {e.GetType().Name}: {e.Message}");
				return false;
			}
		}

		/// <summary>
		/// Mirrors the test plugin sources into the workspace, copying only changed files so that the TypeScript
		/// compiler only reruns when the sources changed.
		/// </summary>
		private void SyncSources()
		{
			Directory.CreateDirectory(workspaceDirectory);
			var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (string source in Directory.GetFiles(sourceDirectory, "*.ts", SearchOption.AllDirectories))
			{
				string target = Path.Combine(workspaceDirectory, Path.GetRelativePath(sourceDirectory, source));
				expected.Add(Path.GetFullPath(target));
				Directory.CreateDirectory(Path.GetDirectoryName(target)!);
				if (File.Exists(target) && File.GetLastWriteTimeUtc(target) >= File.GetLastWriteTimeUtc(source))
					continue;

				File.Copy(source, target, true);
			}

			foreach (string stale in Directory.GetFiles(workspaceDirectory, "*.ts", SearchOption.AllDirectories))
			{
				if (!expected.Contains(Path.GetFullPath(stale))) File.Delete(stale);
			}
		}

		private bool EnsureCompiled()
		{
			var compiler = new PluginAutoCompiler(PluginSystemPaths.SharedConfigDirectory);
			if (!PluginAutoCompiler.NeedsCompile(workspaceDirectory, TestPluginJsEntry))
			{
				report.Log("typescript is up to date");
				return true;
			}

			if (!PluginAutoCompiler.IsTscAvailable())
			{
				report.Error("compile", "tsc is not available in PATH, cannot compile the test plugin");
				return false;
			}

			if (!compiler.Compile(workspaceDirectory, TestPluginEntry, TestPluginJsEntry))
			{
				report.Error("compile", "the test plugin failed to compile; see the tsc output in the console");
				return false;
			}

			report.Log("test plugin compiled");
			return true;
		}

		private bool LoadPlugin()
		{
			var manifest = PrepareManifest();
			if (manifest is null) return false;

			// The bridge initialization of the production path runs through the callback below, so the api it creates
			// is the one the first scenario has to dispose before it resets the chart.
			instance = new PluginInstance(manifest, workspaceDirectory, null,
				bridgeEnv => currentApi = bridgeService.Initialize(bridgeEnv));
			try
			{
				instance.EnsureLoaded();
			}
			catch (Exception e)
			{
				report.Error("load", $"failed to load the test plugin: {e.Message}");
				return false;
			}

			if (instance.State != PluginState.Loaded || instance.Env is null)
			{
				report.Error("load", "the test plugin did not reach the loaded state");
				return false;
			}

			env = instance.Env;
			return true;
		}

		/// <summary>
		/// Writes a manifest compatible with the running API version and reads it back, so the manifest
		/// serialization path is exercised as well.
		/// </summary>
		private PluginManifest? PrepareManifest()
		{
			var parameters = new List<PluginParam>
			{
				new("testText", new I18NString { [Language.English] = "Test text" }, typeof(string)),
				new("testCount", new I18NString { [Language.English] = "Test count" }, typeof(int)),
				new("testFlag", new I18NString { [Language.English] = "Test flag" }, typeof(bool))
			};
			var manifest = new PluginManifest(
				new I18NString { [Language.English] = "Plugin System Tests" },
				parameters,
				false,
				false,
				new I18NString { [Language.English] = "Plugin used to test the plugin system itself" },
				apiVersion,
				TestPluginEntry,
				TestPluginJsEntry);

			string manifestPath = Path.Combine(workspaceDirectory, "manifest.json");
			PluginManifestSerializer.Save(manifestPath, manifest);
			var loaded = PluginManifestSerializer.Load(manifestPath);
			if (loaded?.Name is null)
			{
				report.Error("setup", $"failed to read back the generated manifest: {manifestPath}");
				return null;
			}

			return loaded;
		}

		private void RunScenarios()
		{
			Action<object>? runTests;
			try
			{
				runTests = instance?.Module?.Get<ScriptObject>("default")?.Get<Action<object>>("runTests");
			}
			catch (Exception e)
			{
				report.Error("run", $"failed to read runTests from the test plugin: {e.Message}");
				return;
			}

			if (runTests is null)
			{
				report.Error("run", "the test plugin must expose runTests(harness) on its default export");
				return;
			}

			try
			{
				runTests.Invoke(new PluginTestHarness(this));
			}
			catch (Exception e)
			{
				report.Error("run", $"runTests threw {e.GetType().Name}: {e.Message}");
			}
		}

		private void DisposeRuntime()
		{
			currentContext = null;
			currentApi?.Dispose();
			currentApi = null;
			env = null;
			instance?.Dispose();
			instance = null;
			commands.Dispose();
		}

		private void OnPluginLog(string message, Enum type)
		{
			if (message.StartsWith("EditorPlugin_PluginInternalError", StringComparison.Ordinal))
				pluginErrorLogs.Add(message);
		}

		private void OnUnityLog(string condition, string stackTrace, LogType type)
		{
			if (type is not (LogType.Error or LogType.Exception or LogType.Assert)) return;
			unityErrorLogs.Add($"{type}: {condition}\n{stackTrace}");
		}
	}
}

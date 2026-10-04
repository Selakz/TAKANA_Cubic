#if UNITY_EDITOR
#nullable enable

using System.Collections.Generic;
using EditorPlugin.EditorIntegration;
using EditorPlugin.PluginSystem;
using EditorPlugin.Tests;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.CopyPaste;
using MusicGame.ChartEditor.TrackLine;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace EditorPlugin.Editor
{
	public static class PluginTestMenu
	{
		/// <summary>
		/// Runs the plugin system tests on the current play session. Enter Play Mode in the ChartEditor scene first:
		/// the tests resolve their dependencies from the scene container, because they drive the scene's own chart,
		/// datasets, command manager and views. No editor state is restored afterwards, so a run is meant to end the
		/// play session.
		/// </summary>
		[MenuItem("Tools/Editor Plugin/Run System Tests")]
		public static void RunSystemTests()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("[PluginTest] Enter Play Mode in the ChartEditor scene before running the tests.");
				return;
			}

			var scope = Object.FindFirstObjectByType<HierarchyLifetimeScope>();
			var bridge = Object.FindFirstObjectByType<T3BridgeBootstrapService>();
			var commandManager = Object.FindFirstObjectByType<CommandManager>();
			var manage = Object.FindFirstObjectByType<PluginManageService>();
			if (scope is null || bridge is null || commandManager is null || manage is null)
			{
				Debug.LogError("[PluginTest] Plugin system services not found. " +
				               "Run this while playing the ChartEditor scene.");
				return;
			}

			// The level property and the clipboards are container singletons, so they can only be resolved from
			// the scene scope.
			var levelInfo = scope.Container.Resolve<NotifiableProperty<LevelInfo?>>();
			if (!scope.Container.TryResolve<IDataset<ClipboardItem>>(out var clipboard))
			{
				Debug.LogError("[PluginTest] The editor clipboard is not registered. " +
				               "Run this while playing the ChartEditor scene.");
				return;
			}

			if (!scope.Container.TryResolve<List<NodeRawInfo>>(out var nodeClipboard, "clipboard"))
			{
				Debug.LogError("[PluginTest] The node clipboard is not registered. " +
				               "Run this while playing the ChartEditor scene.");
				return;
			}

			var runner = new PluginTestRunner(levelInfo, commandManager, clipboard, nodeClipboard, bridge, manage,
				manage.ApiVersion?.ToString());
			var report = runner.Run();
			if (report.Succeeded) Debug.Log(report.Format());
			else Debug.LogError(report.Format());
		}
	}
}
#endif

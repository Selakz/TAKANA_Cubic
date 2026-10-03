#nullable enable

using System.IO;
using UnityEngine;

namespace EditorPlugin.PluginSystem
{
	/// <summary>
	/// Locations of the assets shipped with the plugin system and of the test workspace.
	/// </summary>
	public static class PluginSystemPaths
	{
		/// <summary> Directory holding the shared tsconfig.json, types and template provided to plugins. </summary>
		public static string SharedConfigDirectory => Path.Combine(Application.streamingAssetsPath, "EditorPlugin");

		/// <summary>
		/// Sources of the plugin used by the plugin system tests. They live under <c>StreamingAssets/Excluded</c>,
		/// which <c>SelectiveStreamingAssetPacker</c> removes from the project for every build, so they never ship.
		/// </summary>
		public static string TestPluginSourceDirectory =>
			Path.Combine(Application.streamingAssetsPath, "Excluded", "EditorPluginTests");

		/// <summary> Scratch directory the test plugin is copied to and compiled in. </summary>
		public static string TestWorkspaceDirectory => Path.Combine(Application.temporaryCachePath, "PluginSystemTests");
	}
}

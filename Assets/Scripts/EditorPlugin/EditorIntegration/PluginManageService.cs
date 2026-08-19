#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using EditorPlugin.PluginSystem;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.Message;
using MusicGame.Gameplay.Level;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Semver;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Serialization.Json;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace EditorPlugin.EditorIntegration
{
	public interface IPluginManageService
	{
		public SemVersion? ApiVersion { get; }

		public void Refresh();

		public bool LoadZip(string zipPath);

		public bool CreatePlugin(string pluginId, [NotNullWhen(true)] out string? dir);

		public bool DeletePlugin(PluginComponent plugin);
	}

	public class PluginManageService : HierarchySystem<PluginManageService>, IPluginManageService
	{
		// Serializable and Public
		[SerializeField] private PluginApiVersionConfig apiVersionConfig = default!;

		public override bool AsImplementedInterfaces => true;

		public SemVersion? ApiVersion => apiVersionConfig.GetApiVersion(Application.version);

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<string>(ISingleton<EditorSetting>.Instance.PluginPath, () =>
			{
				if (levelInfo.Value is not null) Refresh();
			}),
			new PropertyRegistrar<LevelInfo?>(levelInfo, info =>
			{
				if (info is not null) Refresh();
			})
		};

		// Private
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private IBridgeBootstrapService service = default!;
		[Inject] private IDataset<PluginComponent> dataset = default!;

		[Inject] MessageBox messageBox = default!;

		private readonly PluginAutoCompiler autoCompiler =
			new(Path.Combine(Application.streamingAssetsPath, "EditorPlugin"));

		private static string PluginsRootPath => ISingleton<EditorSetting>.Instance.PluginPath;

		private JsonSerializerSettings SerializerSettings { get; } = new()
		{
			Converters = { new I18NStringJsonConverter(), new PluginParamTypeJsonConverter() },
			ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
		};

		// Defined Functions
		public void Refresh()
		{
			var oldPlugins = dataset.ToList();
			dataset.Clear();
			foreach (var plugin in oldPlugins) plugin.Model.Dispose();

			string rootPath = PluginsRootPath;
			if (!Directory.Exists(rootPath))
			{
				T3Logger.Log("Notice", $"EditorPlugin_PluginPathNotExist|{rootPath}", T3LogType.Error);
				return;
			}

			foreach (string dir in Directory.GetDirectories(rootPath))
			{
				TryLoadPlugin(dir);
			}
		}

		public bool LoadZip(string zipPath)
		{
			string extractDir = Path.Combine(PluginsRootPath, Path.GetFileNameWithoutExtension(zipPath));
			if (Directory.Exists(extractDir)) Directory.Delete(extractDir, true);

			try
			{
				ZipFile.ExtractToDirectory(zipPath, extractDir);
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to extract plugin zip {zipPath}: {e}");
				if (Directory.Exists(extractDir)) Directory.Delete(extractDir, true);
				return false;
			}

			if (TryLoadPlugin(extractDir)) return true;

			Directory.Delete(extractDir, true);
			return false;
		}

		public bool CreatePlugin(string pluginId, [NotNullWhen(true)] out string? dir)
		{
			dir = Path.Combine(PluginsRootPath, pluginId);
			if (Directory.Exists(dir)) return false;

			SemVersion? apiVersion = apiVersionConfig.GetApiVersion(Application.version);
			if (apiVersion is null) return false;

			Directory.CreateDirectory(dir);
			try
			{
				var manifest = new PluginManifest(
					new I18NString { [Language.English] = "New Plugin" },
					Array.Empty<PluginParam>(),
					true,
					true,
					new I18NString { [Language.English] = string.Empty },
					apiVersion.ToString(),
					"main.ts");
				File.WriteAllText(Path.Combine(dir, "manifest.json"),
					JsonConvert.SerializeObject(manifest, SerializerSettings));

				string templatePath = Path.Combine(Application.streamingAssetsPath, "EditorPlugin", "template.ts");
				File.Copy(templatePath, Path.Combine(dir, "main.ts"));
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to create plugin {pluginId}: {e}");
				Directory.Delete(dir, true);
				return false;
			}

			if (TryLoadPlugin(dir)) return true;

			Directory.Delete(dir, true);
			return false;
		}

		public bool DeletePlugin(PluginComponent plugin)
		{
			string dir = plugin.Model.DirectoryPath;
			if (Directory.Exists(dir))
			{
				try
				{
					Directory.Delete(dir, true);
				}
				catch (Exception e)
				{
					Debug.LogError($"Failed to delete plugin directory {dir}: {e}");
					return false;
				}
			}

			dataset.Remove(plugin);
			plugin.Model.Dispose();
			return true;
		}

		private bool TryLoadPlugin(string dir)
		{
			string manifestPath = Path.Combine(dir, "manifest.json");
			if (!File.Exists(manifestPath)) return false;

			try
			{
				PluginManifest? manifest = JsonConvert.DeserializeObject<PluginManifest>(
					File.ReadAllText(manifestPath), SerializerSettings);
				if (manifest?.Name is null) return false;

				if (!apiVersionConfig.IsApiVersionCompatible(manifest.ApiVersion, Application.version))
				{
					T3Logger.Log("Notice",
						$"EditorPlugin_PluginNotCompatible|{manifest.Name.Value}|{manifest.ApiVersion}",
						T3LogType.Error);
					return false;
				}

				string tsEntryPath = Path.Combine(dir, manifest.TsEntry);
				if (!File.Exists(tsEntryPath)) return false;

				string jsEntry = manifest.JsEntry;
				if (PluginAutoCompiler.NeedsCompile(dir, jsEntry))
				{
					if (!PluginAutoCompiler.IsTscAvailable())
					{
						T3Logger.Log("Notice", $"EditorPlugin_TscNotAvailable|{manifest.Name.Value}", T3LogType.Error);
						return false;
					}

					if (!autoCompiler.Compile(dir, manifest.TsEntry, jsEntry))
					{
						T3Logger.Log("Notice", $"EditorPlugin_CompileFailed|{manifest.Name.Value}", T3LogType.Error);
						return false;
					}
				}

				string entryPath = Path.Combine(dir, jsEntry);
				if (!File.Exists(entryPath)) return false;

				string librariesDir = Path.Combine(PluginsRootPath, "libraries");
				string? libraries = Directory.Exists(librariesDir) ? librariesDir : null;

				var instance = new PluginInstance(manifest, dir, libraries, env => service.Initialize(env));
				if (!instance.Manifest.IsLazy) instance.EnsureLoaded();
				dataset.Add(new PluginComponent(instance));
				return true;
			}
			catch (Exception e)
			{
				T3Logger.Log("Notice", $"EditorPlugin_LoadPluginFailed|{dir}", T3LogType.Error);
				messageBox.ShowConfirm(e.Message + "\n" + e.StackTrace, null);
				return false;
			}
		}

		// System Functions
		protected override void OnDestroy()
		{
			base.OnDestroy();
			foreach (var plugin in dataset) plugin.Model.Dispose();
			dataset.Clear();
		}

		void Update()
		{
			foreach (var plugin in dataset)
			{
				if (plugin.Model.Manifest.ShouldTick) plugin.Model.Tick();
			}
		}
	}
}
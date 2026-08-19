#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using EditorPlugin.PluginSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Serialization.Json;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace EditorPlugin.EditorIntegration
{
	public interface IPluginBindInputService
	{
		/// <summary>
		/// Bind hot key to actual input, and try to add the preset to the plugin if it has not been added.
		/// </summary>
		/// <param name="shouldPersistent"> If true, the preset will be saved to disk. </param>
		public void BindInput(PluginComponent plugin, HotkeyPreset hotkeyPreset, bool shouldPersistent);

		public void UnbindInput(PluginComponent plugin, HotkeyPreset hotkeyPreset, bool shouldPersistent);
	}

	public class PluginBindInputService : HierarchySystem<PluginBindInputService>, IPluginBindInputService
	{
		// Serializable and Public
		[SerializeField] private int maxBindCount = 10;

		public override bool AsImplementedInterfaces => true;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars
		{
			get
			{
				var inputRegistrars = new IEventRegistrar[maxBindCount + 1];
				inputRegistrars[0] =
					new DatasetRegistrar<PluginComponent>(dataset,
						DatasetRegistrar<PluginComponent>.RegisterTarget.DataRemoved,
						component => bindings.RemoveAll(x => x.Plugin == component));
				for (int i = 1; i <= maxBindCount; i++)
				{
					var index = i;
					inputRegistrars[index] = new InputRegistrar("EditorPlugin", $"Hotkey{index}", () =>
					{
						foreach (var binding in bindings.Where(binding => binding.Preset.Hotkey == index))
						{
							foreach (var (id, value) in binding.Preset.Params)
							{
								if (binding.Plugin.Model.Params.TryGetValue(id, out var wrapper))
									wrapper.value = value;
							}

							binding.Plugin.Model.Execute();
						}
					});
				}

				return inputRegistrars;
			}
		}

		// Private
		[Inject] private IDataset<PluginComponent> dataset = default!;

		private static readonly JsonSerializerSettings saveSettings = new()
		{
			Formatting = Formatting.Indented,
			Converters = { new I18NStringJsonConverter(), new PluginParamTypeJsonConverter() },
			ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
		};

		private readonly List<(PluginComponent Plugin, HotkeyPreset Preset)> bindings = new();

		// Defined Functions
		public void BindInput(PluginComponent plugin, HotkeyPreset hotkeyPreset, bool shouldPersistent)
		{
			if (!plugin.Model.Manifest.HotkeyPresets.Contains(hotkeyPreset))
			{
				plugin.Model.Manifest.HotkeyPresets.Add(hotkeyPreset);
				plugin.UpdateNotify();
			}

			if (!bindings.Contains((plugin, hotkeyPreset))) bindings.Add((plugin, hotkeyPreset));
			if (shouldPersistent) SaveManifest(plugin.Model);
		}

		public void UnbindInput(PluginComponent plugin, HotkeyPreset hotkeyPreset, bool shouldPersistent)
		{
			if (plugin.Model.Manifest.HotkeyPresets.Contains(hotkeyPreset))
			{
				plugin.Model.Manifest.HotkeyPresets.Remove(hotkeyPreset);
				plugin.UpdateNotify();
			}

			bindings.Remove((plugin, hotkeyPreset));
			if (shouldPersistent) SaveManifest(plugin.Model);
		}

		private static void SaveManifest(PluginInstance plugin)
		{
			string path = Path.Combine(plugin.DirectoryPath, "manifest.json");
			File.WriteAllText(path, JsonConvert.SerializeObject(plugin.Manifest, saveSettings));
		}
	}
}
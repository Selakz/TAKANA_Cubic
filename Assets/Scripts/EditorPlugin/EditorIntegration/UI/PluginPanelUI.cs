#nullable enable

using System.Collections;
using System.Diagnostics;
using SFB;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Plugins;
using T3Framework.Runtime.VContainer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace EditorPlugin.EditorIntegration.UI
{
	public class PluginPanelUI : HierarchySystem<PluginPanelUI>
	{
		// Serializable and Public
		[SerializeField] private I18NTextBlock title = default!;
		[SerializeField] private Button refreshButton = default!;
		[SerializeField] private Button loadZipButton = default!;
		[SerializeField] private TMP_InputField newPluginIdInputField = default!;
		[SerializeField] private Button createNewPluginButton = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DatasetRegistrar<PluginComponent>(dataset,
				DatasetRegistrar<PluginComponent>.RegisterTarget.DataAddedOrRemoved,
				_ => StartCoroutine(RefreshTitle())),
			new ButtonRegistrar(refreshButton, service.Refresh),
			new ButtonRegistrar(loadZipButton, () =>
			{
				var path = FileBrowser.OpenFileDialog("Select plugin zip", "", new ExtensionFilter("Zip Files", "zip"));
				if (!string.IsNullOrEmpty(path) && service.LoadZip(path!))
					T3Logger.Log("Notice", $"EditorPlugin_LoadPluginSuccess|{path}", T3LogType.Success);
				else
					T3Logger.Log("Notice", $"EditorPlugin_LoadPluginFailed|{path}", T3LogType.Warn);
			}),
			new ButtonRegistrar(createNewPluginButton, () =>
			{
				string pluginId = newPluginIdInputField.text;
				if (string.IsNullOrWhiteSpace(pluginId)) return;
				if (service.CreatePlugin(pluginId, out var dir))
				{
					newPluginIdInputField.text = string.Empty;
					T3Logger.Log("Notice", $"EditorPlugin_LoadPluginSuccess|{dir}", T3LogType.Success);
					Process.Start(new ProcessStartInfo
					{
						FileName = dir,
						UseShellExecute = true,
						Verb = "open"
					});
				}
				else
				{
					T3Logger.Log("Notice", $"EditorPlugin_LoadPluginFailed|{pluginId}", T3LogType.Warn);
				}
			})
		};

		// Private
		[Inject] private IPluginManageService service = default!;
		[Inject] private IDataset<PluginComponent> dataset = default!;

		// Defined Functions
		IEnumerator RefreshTitle()
		{
			yield return new WaitForEndOfFrame();
			title.SetText("EditorPlugin_Title", dataset.Count.ToString(), service.ApiVersion?.ToString() ?? "Unknown");
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			title.SetText("EditorPlugin_Title", dataset.Count.ToString(), service.ApiVersion?.ToString() ?? "Unknown");
		}
	}
}
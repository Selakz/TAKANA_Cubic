#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;
#if !UNITY_STANDALONE_WIN && !UNITY_ANDROID && !UNITY_IOS
using T3Framework.Runtime.Plugins;
#endif

namespace App.AutoUpdate.UI
{
	public class InstallButton : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private bool isForce = false;
		[SerializeField] private AutoUpdateWebRequestHandler handler = default!;
		[SerializeField] private Button button = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(button, () =>
			{
#if !UNITY_STANDALONE_WIN && !UNITY_ANDROID && !UNITY_IOS
				FileBrowser.OpenExplorer(Application.streamingAssetsPath);
				return;
#endif
				handler.BeginInstallProcess(isForce);
			})
		};
	}
}
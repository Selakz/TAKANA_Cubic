#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace App.AutoUpdate.UI
{
	public class AutoUpdateDownloadButton : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private AutoUpdateWebRequestHandler handler = default!;
		[SerializeField] private Button button = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(button, () => handler.BeginDownloadProcess().Forget())
		};
	}
}
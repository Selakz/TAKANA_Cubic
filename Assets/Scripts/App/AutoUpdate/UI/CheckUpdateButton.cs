#nullable enable

using Cysharp.Threading.Tasks;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using UnityEngine;
using UnityEngine.UI;

namespace App.AutoUpdate.UI
{
	public class CheckUpdateButton : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private AutoUpdateWebRequestHandler handler = default!;
		[SerializeField] private Button button = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(button, () => OnButtonClick().Forget())
		};

		private async UniTaskVoid OnButtonClick()
		{
			var hasUpdate = await handler.BeginCheckUpdateProcess();
			if (!hasUpdate)
			{
				T3Logger.Log("Notice", "AutoUpdate_Newest", T3LogType.Success);
			}
		}
	}
}
#nullable enable

using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Message;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Utility.AutoUpdate.UI
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
				HeaderMessage.Show("当前版本已为最新！", HeaderMessage.MessageType.Success);
			}
		}
	}
}
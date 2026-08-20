#nullable enable

using MusicGame.ChartEditor.Message;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Level
{
	public class EditorInstaller : HierarchyInstaller
	{
		[SerializeField] private MessageBox messageBox = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterInstance(messageBox);
		}

		// Event Handlers
		private void OnLogMessageRaw(string message)
		{
			messageBox.ShowConfirm(message, null, false);
		}

		// System Functions
		void OnEnable()
		{
			T3Logger.AddListener("MessageRaw", OnLogMessageRaw);
		}

		void OnDisable()
		{
			T3Logger.RemoveListener("MessageRaw", OnLogMessageRaw);
		}
	}
}
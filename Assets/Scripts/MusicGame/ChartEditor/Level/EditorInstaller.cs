#nullable enable

using MusicGame.ChartEditor.Message;
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
	}
}
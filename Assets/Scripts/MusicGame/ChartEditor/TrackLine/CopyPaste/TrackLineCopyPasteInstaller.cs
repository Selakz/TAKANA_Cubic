#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine.CopyPaste
{
	public class TrackLineCopyPasteInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterInstance<List<NodeRawInfo>>(new())
				.Keyed("clipboard");
		}
	}
}
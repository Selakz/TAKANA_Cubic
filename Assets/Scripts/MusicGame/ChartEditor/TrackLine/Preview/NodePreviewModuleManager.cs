#nullable enable

using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine.Preview
{
	public class NodePreviewModuleManager : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority nodePreviewModuleId = default!;
		[SerializeField] private SequencePriority nodePreviewModulePriority = default!;
		[SerializeField] private SequencePriority nodeDragModuleId = default!;
		[SerializeField] private SequencePriority nodeDragModulePriority = default!;
		[SerializeField] private SequencePriority copyPasteModuleId = default!;
		[SerializeField] private SequencePriority copyPasteModulePriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<ChartComponent?>(chartSelectDataset.CurrentSelecting,
				() => { moduleInfo.ModuleModifier.Unregister(nodePreviewModulePriority); }),
			new PropertyRegistrar<EdgeNodeComponent?>(nodeSelectDataset.CurrentSelecting, () =>
			{
				if (nodeSelectDataset.CurrentSelecting.LastValue is null &&
				    nodeSelectDataset.CurrentSelecting.Value is not null)
				{
					moduleInfo.ModuleModifier.Assign(nodePreviewModuleId, nodePreviewModulePriority);
				}
			}),
			new PropertyRegistrar<bool>(nodeDragPlugin.IsDragging, () =>
			{
				var value = nodeDragPlugin.IsDragging.Value;
				if (value) moduleInfo.ModuleModifier.Assign(nodeDragModuleId, nodeDragModulePriority);
				else moduleInfo.ModuleModifier.Unregister(nodeDragModulePriority);
			}),
			new PropertyRegistrar<NodeCopyPastePlugin.PasteMode>(nodeCopyPastePlugin.Mode, () =>
			{
				var mode = nodeCopyPastePlugin.Mode.Value;
				if (mode != NodeCopyPastePlugin.PasteMode.None)
				{
					moduleInfo.ModuleModifier.Register(_ => copyPasteModuleId.Value, copyPasteModulePriority.Value);
				}
				else
				{
					moduleInfo.ModuleModifier.Unregister(copyPasteModulePriority.Value);
				}
			})
		};

		// Private
		private ModuleInfo moduleInfo = default!;
		private ChartSelectDataset chartSelectDataset = default!;
		private EdgeNodeSelectDataset nodeSelectDataset = default!;
		private NodeDragPlugin nodeDragPlugin = default!;
		private NodeCopyPastePlugin nodeCopyPastePlugin = default!;

		// Constructor
		[Inject]
		private void Construct(
			ModuleInfo moduleInfo,
			ChartSelectDataset chartSelectDataset,
			EdgeNodeSelectDataset nodeSelectDataset,
			NodeDragPlugin nodeDragPlugin,
			NodeCopyPastePlugin nodeCopyPastePlugin)
		{
			this.moduleInfo = moduleInfo;
			this.chartSelectDataset = chartSelectDataset;
			this.nodeSelectDataset = nodeSelectDataset;
			this.nodeDragPlugin = nodeDragPlugin;
			this.nodeCopyPastePlugin = nodeCopyPastePlugin;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}
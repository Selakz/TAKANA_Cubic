#nullable enable

using MusicGame.ChartEditor.InScreenEdit.CopyPaste;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.Preview
{
	public class PreviewModuleManager : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority copyPasteModuleId = default!;
		[SerializeField] private SequencePriority copyPasteModulePriority = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<CopyPastePlugin.PasteMode>(copyPastePlugin.Mode, () =>
			{
				var mode = copyPastePlugin.Mode.Value;
				if (mode != CopyPastePlugin.PasteMode.None)
				{
					moduleInfo.ModuleModifier.Register(_ => copyPasteModuleId.Value, copyPasteModulePriority.Value);
				}
				else
				{
					moduleInfo.ModuleModifier.Unregister(copyPasteModulePriority.Value);
				}
			}),
		};

		// Private
		private ModuleInfo moduleInfo = default!;
		private CopyPastePlugin copyPastePlugin = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			ModuleInfo moduleInfo,
			CopyPastePlugin copyPastePlugin)
		{
			this.moduleInfo = moduleInfo;
			this.copyPastePlugin = copyPastePlugin;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}
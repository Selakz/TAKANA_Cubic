#nullable enable

using T3Framework.Runtime.Modifier;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.InScreenEdit
{
	// TODO: Merge Modifier and NotifiableProperty into one class, and extract a interface IReadOnlyNotifiableProperty
	public class ModuleInfo
	{
		private readonly int defaultModule;
		private NotifiableProperty<int>? module;
		private Modifier<int>? modifier;

		public NotifiableProperty<int> CurrentModule => module ??= new(defaultModule);

		public Modifier<int> ModuleModifier => modifier ??= new(
			() => CurrentModule.Value,
			value => CurrentModule.Value = value,
			_ => defaultModule);

		public ModuleInfo(int defaultModule)
		{
			this.defaultModule = defaultModule;
		}

		public void Register(int moduleId) => ModuleModifier.Assign(moduleId, moduleId);

		public void Unregister(int moduleId) => ModuleModifier.Unregister(moduleId);
	}
}
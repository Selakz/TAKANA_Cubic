#nullable enable

using System;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit.CopyPaste
{
	public enum PasteMode
	{
		NormalPaste,
		ExactPaste
	}

	public class ClipboardItem : IComponent
	{
		public ChartComponent Component { get; set; }

		public ChartComponent? Parent { get; set; }

		public ClipboardItem(ChartComponent component, ChartComponent? parent)
		{
			Component = component;
			Parent = parent;
		}

		public event EventHandler? OnComponentUpdated;

		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);

		public static ClipboardItem FromComponent(ChartComponent component)
		{
			var token = component.GetSerializationToken();
			var clonedComponent = ChartComponent.Deserialize(token, null!);
			return new ClipboardItem(clonedComponent, component.Parent);
		}
	}

	public class CopyPasteInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterNotifiableProperty(PasteMode.NormalPaste);
			builder.Register<IDataset<ClipboardItem>, HashDataset<ClipboardItem>>(Lifetime.Singleton);
		}
	}
}
#nullable enable

using MusicGame.ChartEditor.InScreenEdit.Grid;
using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class InScreenEditInstaller : HierarchyInstaller
	{
		[SerializeField] private SequencePriority defaultModule = default!;
		[SerializeField] private SequencePriority chartEditPriority = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			// TimeRetriever
			builder.Register<ITimeRetriever, DefaultTimeRetriever>(Lifetime.Singleton);
			builder.Register<GridTimeRetriever>(Lifetime.Singleton);
			builder.Register<NotifiableProperty<ITimeRetriever>>(Lifetime.Singleton);
			builder.Register<StageMouseTimeRetriever>(Lifetime.Singleton);
			// WidthRetriever
			builder.Register<IWidthRetriever, DefaultWidthRetriever>(Lifetime.Singleton);
			builder.Register<NotifiableProperty<IWidthRetriever>>(Lifetime.Singleton);
			builder.Register<StageMouseWidthRetriever>(Lifetime.Singleton);

			builder.RegisterEntryPoint<ChartEditSystem>()
				.AsSelf();
			builder.RegisterEntryPoint<ChartEditInputSystem>()
				.AsSelf()
				.WithParameter("chartEditPriority", chartEditPriority.Value);

			// ModuleInfo
			builder.Register<ModuleInfo>(Lifetime.Singleton)
				.WithParameter("defaultModule", defaultModule.Value);
		}
	}
}
#nullable enable

using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.Models;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public enum SingleNotePlaceType
	{
		SelectedTrack,
		NearestTrack
	}

	public class InScreenEditInstaller : HierarchyInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority defaultModule = default!;

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

			// Note Editing
			builder.RegisterNotifiableProperty(T3Flag.Tap);
			builder.RegisterNotifiableProperty(SingleNotePlaceType.SelectedTrack);
			builder.Register<IDataset<NoteRawInfo>, HashDataset<NoteRawInfo>>(Lifetime.Singleton);

			// Track Editing
			builder.Register<IDataset<TrackRawInfo>, HashDataset<TrackRawInfo>>(Lifetime.Singleton);

			// ModuleInfo
			builder.Register<ModuleInfo>(Lifetime.Singleton)
				.WithParameter("defaultModule", defaultModule.Value);
		}
	}
}
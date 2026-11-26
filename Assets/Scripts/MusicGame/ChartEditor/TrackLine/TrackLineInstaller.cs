#nullable enable

using MusicGame.ChartEditor.Decoration.Track;
using T3Framework.Preset.Select;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	public enum NodeCurveType
	{
		Ease,
		Bezier
	}

	public class PreviewNodeClassifier : IClassifier<TrackNodeType>
	{
		public static PreviewNodeClassifier Instance { get; } = new();

		public TrackNodeType Classify(IComponent component)
			=> component is NodeRawInfo info ? info.Type.Value : TrackNodeType.Left;

		public bool IsOfType(IComponent component, TrackNodeType type) => Classify(component) == type;

		public bool IsSubType(TrackNodeType subType, TrackNodeType type) => subType == type;
	}

	public class TrackLineInstaller : HierarchyInstaller
	{
		// Serializable and Public
		[SerializeField] private ClassViewPoolInstaller<TrackNodeType> previewPoolInstaller;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterNotifiableProperty(NodeCurveType.Ease);
			builder.Register<IDataset<NodeRawInfo>, HashDataset<NodeRawInfo>>(Lifetime.Singleton);
			builder.RegisterClassViewPool<NodeRawInfo, TrackNodeType>(previewPoolInstaller, PreviewNodeClassifier.Instance);

			// SelectInputSystem
			builder.RegisterInstance(new NotifiableProperty<ISelectRaycastMode<TrackNodeComponent>>
				(PollingRaycastMode<TrackNodeComponent>.InstanceSole)).AsSelf();
			builder.RegisterInstance(new NotifiableProperty<bool>(false))
				.AsSelf().Keyed("create-node-state");
			builder.RegisterInstance(new NotifiableProperty<int>(ISingleton<TrackLineSetting>.Instance.DefaultEaseId))
				.AsSelf().Keyed("ease-id");
		}
	}
}

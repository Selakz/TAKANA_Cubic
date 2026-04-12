#nullable enable

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

	public class PreviewNodeClassifier : IClassifier<NodeType>
	{
		public static PreviewNodeClassifier Instance { get; } = new();

		public NodeType Classify(IComponent component)
			=> component is NodeRawInfo info ? info.Type.Value : NodeType.Left;

		public bool IsOfType(IComponent component, NodeType type) => Classify(component) == type;

		public bool IsSubType(NodeType subType, NodeType type) => subType == type;
	}

	public class TrackLineInstaller : HierarchyInstaller
	{
		// Serializable and Public
		[SerializeField] private ClassViewPoolInstaller<NodeType> previewPoolInstaller;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterNotifiableProperty(NodeCurveType.Ease);
			builder.Register<IDataset<NodeRawInfo>, HashDataset<NodeRawInfo>>(Lifetime.Singleton);
			builder.RegisterClassViewPool<NodeRawInfo, NodeType>(previewPoolInstaller, PreviewNodeClassifier.Instance);

			// SelectInputSystem
			builder.RegisterInstance(new NotifiableProperty<ISelectRaycastMode<EdgeNodeComponent>>
				(PollingRaycastMode<EdgeNodeComponent>.InstanceSole)).AsSelf();
			builder.RegisterInstance(new NotifiableProperty<ISelectRaycastMode<DirectNodeComponent>>
				(PollingRaycastMode<DirectNodeComponent>.InstanceSole)).AsSelf();
			builder.RegisterInstance(new NotifiableProperty<bool>(false))
				.AsSelf().Keyed("create-node-state");
			builder.RegisterInstance(new NotifiableProperty<int>(ISingleton<TrackLineSetting>.Instance.DefaultEaseId))
				.AsSelf().Keyed("ease-id");
		}
	}
}
#nullable enable

using MusicGame.Models;
using T3Framework.Preset.Drag;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public interface IComponentDragHelper : IDragHelper
	{
		public void Prepare(IChartModel model);

		public IChartModel? BaseModel { get; }
	}

	public class OverridableComponentDragHelper : IComponentDragHelper
	{
		// Serializable and Public
		public NotifiableProperty<bool> IsDragging { get; } = new(false);

		public IChartModel? BaseModel => Current.Value.BaseModel;

		public NotifiableProperty<IComponentDragHelper> Current => property ??= new(defaultDragHelper);

		// Private
		private readonly IComponentDragHelper defaultDragHelper;
		private NotifiableProperty<IComponentDragHelper>? property;
		private PropertyRegistrar<bool>? isDraggingRegistrar;

		// Defined Functions
		public OverridableComponentDragHelper(IComponentDragHelper defaultDragHelper)
		{
			this.defaultDragHelper = defaultDragHelper;
			isDraggingRegistrar = new(defaultDragHelper.IsDragging, value => IsDragging.Value = value);
			isDraggingRegistrar?.Register();
		}

		public void Prepare(IChartModel model)
		{
			Current.Value.Prepare(model);
		}

		// Thought: IEventRegistrar is kind of Lazy<IDisposable>, but is actually a worse design than that. I should learn reactive earlier...
		public IEventRegistrar OverrideDragHelper(IComponentDragHelper dragHelper)
		{
			return new CustomRegistrar(
				() =>
				{
					if (Current.Value.IsDragging) Current.Value.CancelDrag();
					Current.Value = dragHelper;
					isDraggingRegistrar?.Unregister();
					isDraggingRegistrar = new(dragHelper.IsDragging, value => IsDragging.Value = value);
					isDraggingRegistrar?.Register();
				},
				() =>
				{
					if (Current.Value == dragHelper)
					{
						Current.Value = defaultDragHelper;
						isDraggingRegistrar?.Unregister();
						isDraggingRegistrar = new(defaultDragHelper.IsDragging, value => IsDragging.Value = value);
						isDraggingRegistrar?.Register();
					}
				});
		}

		public bool BeginDrag() => Current.Value.BeginDrag();

		public bool EndDrag() => Current.Value.EndDrag();

		public void CancelDrag() => Current.Value.CancelDrag();
	}
}
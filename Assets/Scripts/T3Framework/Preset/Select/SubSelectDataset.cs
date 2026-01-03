#nullable enable

using System.ComponentModel;
using System.Linq;
using T3Framework.Runtime.ECS;
using T3Framework.Static.Event;
using IComponent = T3Framework.Runtime.ECS.IComponent;

namespace T3Framework.Preset.Select
{
	public class SubSelectDataset<T, TClass> : SubDataset<T, TClass>, ISelectDataset<T> where T : IComponent
	{
		private readonly ISelectDataset<T> parentDataset;

		public NotifiableProperty<T?> CurrentSelecting { get; } = new(default);

		public SubSelectDataset(ISelectDataset<T> parentDataset, IClassifier<TClass> classifier,
			params TClass[] targetClasses) : base(parentDataset, classifier, targetClasses)
		{
			this.parentDataset = parentDataset;
			this.parentDataset.CurrentSelecting.PropertyChanged += ParentCurrentSelectingChanged;
			var value = parentDataset.CurrentSelecting.Value;
			if (value is null || !IsDataOfClass(value)) CurrentSelecting.Value = default;
			else CurrentSelecting.Value = value;
		}

		public bool Add(T item) => IsDataOfClass(item) && parentDataset.Add(item);

		public bool Remove(T item) => IsDataOfClass(item) && parentDataset.Remove(item);

		public void Clear()
		{
			var data = this.ToArray();
			foreach (var item in data) parentDataset.Remove(item);
		}

		public void ToggleSelecting(T component)
		{
			if (!IsDataOfClass(component)) return;
			parentDataset.ToggleSelecting(component);
		}

		private void ParentCurrentSelectingChanged(object sender, PropertyChangedEventArgs e)
		{
			var value = parentDataset.CurrentSelecting.Value;
			if (value is null || !IsDataOfClass(value)) CurrentSelecting.Value = default;
			else CurrentSelecting.Value = value;
		}

		public override void Dispose()
		{
			base.Dispose();
			parentDataset.CurrentSelecting.PropertyChanged -= ParentCurrentSelectingChanged;
		}
	}
}
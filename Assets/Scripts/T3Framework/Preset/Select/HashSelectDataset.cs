#nullable enable

using System.Collections.Generic;
using System.Linq;
using T3Framework.Runtime.ECS;
using T3Framework.Static.Event;

namespace T3Framework.Preset.Select
{
	public class HashSelectDataset<T> : RootDataset<T>, ISelectDataset<T> where T : IComponent
	{
		private readonly HashSet<T> dataset = new();

		public NotifiableProperty<T?> CurrentSelecting { get; } = new(default);

		public override int Count => dataset.Count;

		public override bool Contains(T item) => dataset.Contains(item);

		public void ToggleSelecting(T component)
		{
			if (Contains(component)) Remove(component);
			else Add(component);
		}

		protected override bool AddToDataset(T item)
		{
			if (dataset.Add(item))
			{
				CurrentSelecting.Value = item;
				return true;
			}

			return false;
		}

		protected override bool IsRemovable(T item) => dataset.Contains(item);

		protected override void RemoveFromDataset(T item)
		{
			dataset.Remove(item);
			if (Equals(CurrentSelecting.Value, item)) CurrentSelecting.Value = this.FirstOrDefault();
		}

		protected override bool NeedToRemove(T item) => false;

		public override IEnumerator<T> GetEnumerator() => dataset.GetEnumerator();
	}
}
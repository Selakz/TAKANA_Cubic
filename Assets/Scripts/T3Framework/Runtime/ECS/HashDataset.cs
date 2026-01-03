#nullable enable

using System.Collections.Generic;

namespace T3Framework.Runtime.ECS
{
	public class HashDataset<T> : RootDataset<T> where T : IComponent
	{
		private readonly HashSet<T> dataset = new();

		public override int Count => dataset.Count;

		public override bool Contains(T item) => dataset.Contains(item);

		protected override bool AddToDataset(T item) => dataset.Add(item);

		protected override bool IsRemovable(T item) => dataset.Contains(item);

		protected override void RemoveFromDataset(T item) => dataset.Remove(item);

		protected override bool NeedToRemove(T item) => false;

		public override IEnumerator<T> GetEnumerator() => dataset.GetEnumerator();
	}
}
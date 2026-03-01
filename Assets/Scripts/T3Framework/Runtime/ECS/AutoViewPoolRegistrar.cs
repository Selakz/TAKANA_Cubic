#nullable enable

using T3Framework.Runtime.Event;

namespace T3Framework.Runtime.ECS
{
	public class AutoViewPoolRegistrar<T> : IEventRegistrar where T : IComponent
	{
		private readonly IDataset<T> dataset;
		private readonly IViewPool<T> viewPool;
		private readonly bool isCovering;

		public AutoViewPoolRegistrar(IDataset<T> dataset, IViewPool<T> viewPool, bool isCovering = false)
		{
			this.dataset = dataset;
			this.viewPool = viewPool;
			this.isCovering = isCovering;
		}

		public void Register()
		{
			dataset.OnDataAdded += OnDataAdded;
			dataset.BeforeDataRemoved += OnDataRemoved;
			if (isCovering)
			{
				foreach (var data in dataset) viewPool.Add(data);
			}
		}

		public void Unregister()
		{
			dataset.OnDataAdded -= OnDataAdded;
			dataset.BeforeDataRemoved -= OnDataRemoved;
			viewPool.Clear();
		}

		private void OnDataAdded(T data) => viewPool.Add(data);

		private void OnDataRemoved(T data) => viewPool.Remove(data);
	}
}
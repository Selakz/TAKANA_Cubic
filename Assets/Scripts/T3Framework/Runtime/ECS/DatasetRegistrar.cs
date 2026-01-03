#nullable enable

using System;
using T3Framework.Runtime.Event;

namespace T3Framework.Runtime.ECS
{
	public readonly struct DatasetRegistrar<T> : IEventRegistrar where T : IComponent
	{
		public enum RegisterTarget
		{
			DataAdded,
			DataRemoved,
			DataUpdated,
			DataAddedOrRemoved
		}

		private readonly IReadOnlyDataset<T> dataset;
		private readonly RegisterTarget target;
		private readonly Action<T> handler;

		public DatasetRegistrar(IReadOnlyDataset<T> dataset, RegisterTarget target, Action<T> handler)
		{
			this.dataset = dataset;
			this.target = target;
			this.handler = handler;
		}

		public void Register()
		{
			switch (target)
			{
				case RegisterTarget.DataAdded:
					dataset.OnDataAdded += handler;
					break;
				case RegisterTarget.DataRemoved:
					dataset.BeforeDataRemoved += handler;
					break;
				case RegisterTarget.DataUpdated:
					dataset.OnDataUpdated += handler;
					break;
				case RegisterTarget.DataAddedOrRemoved:
					dataset.OnDataAdded += handler;
					dataset.BeforeDataRemoved += handler;
					break;
			}
		}

		public void Unregister()
		{
			switch (target)
			{
				case RegisterTarget.DataAdded:
					dataset.OnDataAdded -= handler;
					break;
				case RegisterTarget.DataRemoved:
					dataset.BeforeDataRemoved -= handler;
					break;
				case RegisterTarget.DataUpdated:
					dataset.OnDataUpdated -= handler;
					break;
				case RegisterTarget.DataAddedOrRemoved:
					dataset.OnDataAdded -= handler;
					dataset.BeforeDataRemoved -= handler;
					break;
			}
		}
	}
}
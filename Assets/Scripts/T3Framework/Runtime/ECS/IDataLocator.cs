#nullable enable

using System;

namespace T3Framework.Runtime.ECS
{
	public interface IDataLocator<TData> : IEquatable<IDataLocator<TData>>
	{
		public TData? GetData();
	}
}
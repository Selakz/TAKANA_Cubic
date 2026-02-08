#nullable enable

namespace T3Framework.Runtime.Modifier
{
	public interface IModifier<T>
	{
		public T Value { get; }

		public void Assign(T value, int priority);

		public void Unregister(int priority);

		public void Clear();
	}
}
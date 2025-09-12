using UnityEngine;

namespace T3Framework.Runtime.MVC
{
	public interface IModelRetrievable
	{
		public IModel GenericModel { get; }
	}

	public interface IModelRetrievable<out T> where T : IModel
	{
		public T Model { get; }
	}

	public interface IModelRequired<in T> where T : IModel
	{
		public void Init(T model);
	}

	public interface IController<T> : IModelRetrievable, IModelRetrievable<T>, IModelRequired<T> where T : IModel
	{
		public GameObject Object { get; }

		public void Destroy();
	}
}
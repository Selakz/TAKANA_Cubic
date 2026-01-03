#nullable enable

using System;

namespace T3Framework.Runtime.ECS
{
	public interface IComponent
	{
		public event EventHandler? OnComponentUpdated;

		public void UpdateNotify();
	}

	/// <summary>
	/// A component that contains a model.
	/// </summary>
	public interface IComponent<out T> : IComponent
	{
		/// <summary> "read only". Any modification of it should go <see cref="UpdateModel"/> </summary>
		T Model { get; }

		/// <summary> Update the content of the model and trigger update events accordingly. </summary>
		public void UpdateModel(Action<T> action);
	}

	public class BaseComponent<T> : IComponent<T>
	{
		private T model;

		public event EventHandler? OnComponentUpdated;

		public T Model
		{
			get => model;
			set
			{
				model = value;
				UpdateNotify();
			}
		}

		public BaseComponent(T model) => this.model = model;

		public void UpdateModel(Action<T> action)
		{
			action.Invoke(model);
			UpdateNotify();
		}

		public void UpdateNotify()
		{
			OnComponentUpdated?.Invoke(this, EventArgs.Empty);
		}
	}
}
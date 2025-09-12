using T3Framework.Runtime.Event;
using T3Framework.Runtime.MVC;
using UnityEngine;

namespace MusicGame.ChartEditor.Select
{
	public interface IModelSelectable
	{
		public IModel Model { get; }
	}

	/// <summary>
	/// Selecting is only used to highlight in view layer.
	/// </summary>
	/// <typeparam name="T"> The EXACT type of model, since <see cref="GameObject.GetComponent{T}"/> cannot get interface with covariant generic type.</typeparam>
	public abstract class SimpleModelSelectable<T> : MonoBehaviour, IModelSelectable where T : IModel
	{
		// Serializable and Public
		public IModel Model => modelRetrievable.Model;

		private bool IsSelected => ISelectManager.Instance.IsSelected(Model.Id);

		// Private
		private IModelRetrievable<T> modelRetrievable;

		// Static

		// Defined Functions
		protected abstract void Highlight();
		protected abstract void Unhighlight();

		// Event Handlers
		private void SelectionOnUpdate()
		{
			if (IsSelected) Highlight();
			else Unhighlight();
		}

		// System Functions
		protected virtual void Awake()
		{
			modelRetrievable = GetComponent<IModelRetrievable<T>>();
			if (modelRetrievable == null)
			{
				Debug.LogError("Cannot retrieve model on this game object");
			}

			else if (modelRetrievable.Model == null)
			{
				Debug.LogError("Cannot retrieve model from model retrievable, but why?");
			}
		}

		protected virtual void OnEnable()
		{
			EventManager.Instance.AddListener("Selection_OnUpdate", SelectionOnUpdate);
			SelectionOnUpdate();
		}

		protected virtual void OnDisable()
		{
			EventManager.Instance.RemoveListener("Selection_OnUpdate", SelectionOnUpdate);
		}
	}
}
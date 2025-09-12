using MusicGame.Components.Chart;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.MVC;
using UnityEngine;

namespace MusicGame.Components.JudgeLines
{
	public class JudgeLineController : MonoBehaviour, IController<JudgeLine>
	{
		// Serializable and Public
		[SerializeField] private Transform childrenRoot;
		[SerializeField] private Transform sprite;
		[SerializeField] private Transform leftEdge;
		[SerializeField] private Transform rightEdge;

		public JudgeLine Model { get; private set; }

		public IModel GenericModel => Model;

		public GameObject Object => gameObject;

		// Private

		// Static

		// Defined Function
		public void Init(JudgeLine model)
		{
			Model = model;
			sprite.SetPositionAndRotation(Model.Position, model.Rotation);
		}

		public void Destroy()
		{
			foreach (Transform children in childrenRoot)
			{
				if (children.TryGetComponent(out IModelRetrievable r))
				{
					r.GenericModel.Destroy();
				}
			}

			Model.Controller = null;
			Destroy(Object);
		}

		// Event Handlers
		private void ChartOnUpdate(ChartInfo chartInfo)
		{
			if (!chartInfo.Contains(Model.Id))
			{
				Model.Destroy();
			}
		}

		// System Function
		void OnEnable()
		{
			EventManager.Instance.AddListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
		}
	}
}
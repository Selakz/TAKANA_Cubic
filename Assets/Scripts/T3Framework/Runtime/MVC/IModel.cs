using UnityEngine;

namespace T3Framework.Runtime.MVC
{
	public interface IControllerRetrievable
	{
		MonoBehaviour Controller { get; }
	}

	public interface IControllerRetrievable<out T> where T : MonoBehaviour
	{
		T Controller { get; }
	}

	public interface IModel : IControllerRetrievable
	{
		public int Id { get; }

		public GameObject Prefab { get; }

		public Vector3 Position { get; set; }

		public bool Generate();

		public bool Destroy();
	}
}
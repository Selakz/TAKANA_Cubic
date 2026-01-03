#nullable enable

using System;
using UnityEngine;

namespace T3Framework.Runtime.Event.UI
{
	[RequireComponent(typeof(Collider))]
	public class ColliderEnterExitListener : MonoBehaviour
	{
		// Serializable and Public
		public event Action<Collider>? CollisionEnter;
		public event Action<Collider>? CollisionExit;

		public Collider Collider => targetCollider ??= GetComponent<Collider>();

		public int Layer
		{
			get => gameObject.layer;
			set => gameObject.layer = value;
		}

		// Private
		private Collider? targetCollider;

		// System Functions
		public void OnCollisionEnter(Collision other)
		{
			Debug.Log($"OnCollisionEnter: {other.gameObject.name}");
			CollisionEnter?.Invoke(other.collider);
		}

		public void OnCollisionExit(Collision other)
		{
			CollisionExit?.Invoke(other.collider);
		}

		public void OnTriggerEnter(Collider other)
		{
			CollisionEnter?.Invoke(other);
		}

		public void OnTriggerExit(Collider other)
		{
			CollisionExit?.Invoke(other);
		}
	}
}
#nullable enable

using System;
using UnityEngine;

namespace T3Framework.Runtime.Event.UI
{
	[RequireComponent(typeof(Collider2D))]
	public class Collider2DEnterExitListener : MonoBehaviour
	{
		// Serializable and Public
		public event Action<Collider2D>? CollisionEnter;
		public event Action<Collider2D>? CollisionExit;

		public Collider2D Collider => targetCollider ??= GetComponent<Collider2D>();

		public int Layer
		{
			get => gameObject.layer;
			set => gameObject.layer = value;
		}

		// Private
		private Collider2D? targetCollider;

		// System Functions
		public void OnCollisionEnter2D(Collision2D other)
		{
			CollisionEnter?.Invoke(other.collider);
		}

		public void OnCollisionExit2D(Collision2D other)
		{
			CollisionExit?.Invoke(other.collider);
		}

		public void OnTriggerEnter2D(Collider2D other)
		{
			CollisionEnter?.Invoke(other);
		}

		public void OnTriggerExit2D(Collider2D other)
		{
			CollisionExit?.Invoke(other);
		}
	}

	public class Collider2DEnterExitRegistrar : IEventRegistrar
	{
		public enum RegisterTarget
		{
			CollisionEnter,
			CollisionExit
		}

		private readonly Collider2DEnterExitListener listener;
		private readonly RegisterTarget target;
		private readonly Action<Collider2D> action;

		public Collider2DEnterExitRegistrar(
			Collider2DEnterExitListener listener, RegisterTarget target, Action<Collider2D> action)
		{
			this.listener = listener;
			this.target = target;
			this.action = action;
		}

		public void Register()
		{
			switch (target)
			{
				case RegisterTarget.CollisionEnter:
					listener.CollisionEnter += action;
					break;
				case RegisterTarget.CollisionExit:
					listener.CollisionExit += action;
					break;
			}
		}

		public void Unregister()
		{
			switch (target)
			{
				case RegisterTarget.CollisionEnter:
					listener.CollisionEnter -= action;
					break;
				case RegisterTarget.CollisionExit:
					listener.CollisionExit -= action;
					break;
			}
		}
	}
}
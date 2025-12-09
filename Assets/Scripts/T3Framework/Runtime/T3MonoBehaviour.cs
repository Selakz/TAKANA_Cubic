#nullable enable

using System;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace T3Framework.Runtime
{
	/// <summary> Inherit <see cref="MonoBehaviour"/> to provide more functions within T3Framework conveniently. </summary>
	public abstract class T3MonoBehaviour : MonoBehaviour
	{
		// Serializable and Public
		protected virtual IEventRegistrar[] AwakeRegistrars { get; } = Array.Empty<IEventRegistrar>();

		protected virtual IEventRegistrar[] EnableRegistrars { get; } = Array.Empty<IEventRegistrar>();

		// Private
		private IEventRegistrar[]? awakeRegistrars;
		private IEventRegistrar[]? enableRegistrars;

		// System Functions
		protected virtual void Awake()
		{
			awakeRegistrars ??= AwakeRegistrars;
			foreach (var registrar in awakeRegistrars) registrar.Register();
		}

		protected virtual void OnEnable()
		{
			enableRegistrars ??= EnableRegistrars;
			foreach (var registrar in enableRegistrars) registrar.Register();
		}

		protected virtual void OnDisable()
		{
			enableRegistrars ??= EnableRegistrars;
			foreach (var registrar in enableRegistrars) registrar.Unregister();
		}

		protected virtual void OnDestroy()
		{
			awakeRegistrars ??= AwakeRegistrars;
			foreach (var registrar in awakeRegistrars) registrar.Unregister();
		}
	}
}
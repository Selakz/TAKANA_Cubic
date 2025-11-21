#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace T3Framework.Runtime
{
	/// <summary> Inherit <see cref="MonoBehaviour"/> to provide more functions within T3Framework conveniently. </summary>
	public abstract class T3MonoBehaviour : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private string[] inspectorTags = { };

		/// <summary> Will only be called once when <see cref="T3MonoBehaviour.Tags"/> is constructed </summary>
		protected virtual string[] PresetTags => new string[] { };

		/// <summary> A single tag of GameObject should never be enough! </summary>
		public EventTags Tags
		{
			get
			{
				if (tags is null)
				{
					var initialTags = new HashSet<string>();
					initialTags.UnionWith(inspectorTags);
					initialTags.UnionWith(PresetTags);
					tags = new(gameObject, initialTags);
				}

				return tags;
			}
		}

		protected virtual IEventRegistrar[] AwakeRegistrars { get; } = Array.Empty<IEventRegistrar>();

		protected virtual IEventRegistrar[] EnableRegistrars { get; } = Array.Empty<IEventRegistrar>();

		// Private
		private EventTags? tags;
		private IEventRegistrar[]? awakeRegistrars;
		private IEventRegistrar[]? enableRegistrars;

		// System Functions
		protected virtual void Awake()
		{
			if (tags is null)
			{
				var initialTags = new HashSet<string>();
				initialTags.UnionWith(inspectorTags);
				initialTags.UnionWith(PresetTags);
				tags = new(gameObject, initialTags);
			}

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

			tags?.Dispose();
		}
	}
}
#nullable enable

using System;
using System.ComponentModel;
using T3Framework.Runtime.Event;
using T3Framework.Static.Event;
using VContainer.Unity;

namespace T3Framework.Runtime.ECS
{
	// TODO: Parent System
	public abstract class T3System : IInitializable, IPostInitializable, IDisposable
	{
		protected virtual IEventRegistrar[] InitializeRegistrars { get; } = Array.Empty<IEventRegistrar>();
		protected virtual IEventRegistrar[] ActiveRegistrars { get; } = Array.Empty<IEventRegistrar>();
		private IEventRegistrar[]? initializeRegistrars;
		private IEventRegistrar[]? activeRegistrars;

		protected T3System(bool isStartActive) => this.isStartActive = isStartActive;

		public NotifiableProperty<bool> IsActive => isActive ??= new(false);
		private NotifiableProperty<bool>? isActive;

		private readonly bool isStartActive;

		public virtual void Initialize()
		{
			IsActive.PropertyChanged += OnActiveStateChanged;
			initializeRegistrars ??= InitializeRegistrars;
			foreach (var registrar in initializeRegistrars) registrar.Register();
		}

		public virtual void PostInitialize()
		{
			if (isStartActive) IsActive.Value = true;
		}

		protected virtual void OnActive()
		{
			activeRegistrars ??= ActiveRegistrars;
			foreach (var registrar in activeRegistrars) registrar.Register();
		}

		protected virtual void OnInactive()
		{
			activeRegistrars ??= ActiveRegistrars;
			foreach (var registrar in activeRegistrars) registrar.Unregister();
		}

		public virtual void Dispose()
		{
			IsActive.Value = false;
			initializeRegistrars ??= InitializeRegistrars;
			foreach (var registrar in initializeRegistrars) registrar.Unregister();
			IsActive.PropertyChanged -= OnActiveStateChanged;
		}

		private void OnActiveStateChanged(object sender, PropertyChangedEventArgs e)
		{
			var active = IsActive.Value;
			if (active) OnActive();
			else OnInactive();
		}
	}
}
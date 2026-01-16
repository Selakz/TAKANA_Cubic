#nullable enable

namespace T3Framework.Runtime.Event
{
	public abstract class CompositeRegistrar : IEventRegistrar
	{
		private IEventRegistrar[]? innerRegistrars = null;

		protected abstract IEventRegistrar[] InnerRegistrars { get; }

		protected abstract void Initialize();

		protected abstract void Deinitialize();

		public void Register()
		{
			Initialize();
			innerRegistrars ??= InnerRegistrars;
			foreach (var registrar in innerRegistrars) registrar.Register();
		}

		public void Unregister()
		{
			Deinitialize();
			innerRegistrars ??= InnerRegistrars;
			foreach (var registrar in innerRegistrars) registrar.Unregister();
		}
	}
}
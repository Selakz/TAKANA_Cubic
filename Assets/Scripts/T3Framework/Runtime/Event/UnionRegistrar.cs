#nullable enable

using System;

namespace T3Framework.Runtime.Event
{
	public class UnionRegistrar : IEventRegistrar
	{
		private readonly IEventRegistrar[] registrars;

		public UnionRegistrar(params IEventRegistrar[] registrars)
		{
			this.registrars = registrars;
		}

		public void Register()
		{
			foreach (var registrar in registrars) registrar.Register();
		}

		public void Unregister()
		{
			Array.Reverse(registrars);
			foreach (var registrar in registrars) registrar.Unregister();
			Array.Reverse(registrars);
		}
	}
}
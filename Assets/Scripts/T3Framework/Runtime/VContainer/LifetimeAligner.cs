#nullable enable

using T3Framework.Runtime.Event;
using T3Framework.Runtime.Event.UI;
using UnityEngine;

namespace T3Framework.Runtime.VContainer
{
	public class LifetimeAligner : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private LifetimeListener listener = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new LifetimeRegistrar(listener, active => gameObject.SetActive(active))
		};
	}
}
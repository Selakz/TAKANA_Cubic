#nullable enable

using T3Framework.Runtime.Event;
using UnityEngine;

namespace T3Framework.Preset.DataContainers
{
	public class StringDataContainer : NotifiableDataContainer<string>
	{
		// Serializable and Public
		[SerializeField] private string initialValue = string.Empty;

		public override string InitialValue => initialValue;
	}
}
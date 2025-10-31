#nullable enable

using T3Framework.Static.Event;
using UnityEngine;

namespace T3Framework.Preset.DataContainers
{
	public class IntegerDataContainer : NotifiableDataContainer<int>
	{
		// Serializable and Public
		[SerializeField] private int initialValue = 0;

		public override int InitialValue => initialValue;
	}
}
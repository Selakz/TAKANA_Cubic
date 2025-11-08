#nullable enable

using T3Framework.Runtime.Event;
using UnityEngine;

namespace T3Framework.Preset.DataContainers
{
	public class BoolDataContainer : NotifiableDataContainer<bool>
	{
		[SerializeField] private bool isInitiallyTrue = false;

		public override bool InitialValue => isInitiallyTrue;
	}
}
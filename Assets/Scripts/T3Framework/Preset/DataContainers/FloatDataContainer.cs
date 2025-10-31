#nullable enable

using T3Framework.Static.Event;
using UnityEngine;

namespace T3Framework.Preset.DataContainers
{
	public class FloatDataContainer : NotifiableDataContainer<float>
	{
		// Serializable and Public
		[SerializeField] private float initialValue;

		public override float InitialValue => initialValue;
	}
}
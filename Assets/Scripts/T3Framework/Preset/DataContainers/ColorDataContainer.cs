#nullable enable

using T3Framework.Static.Event;
using UnityEngine;

namespace T3Framework.Preset.DataContainers
{
	public class ColorDataContainer : NotifiableDataContainer<Color>
	{
		// Serializable and Public
		[SerializeField] private Color initialColor = Color.white;

		public override Color InitialValue => initialColor;
	}
}
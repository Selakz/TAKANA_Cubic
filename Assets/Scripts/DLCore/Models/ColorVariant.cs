#nullable enable

namespace DLCore.Models
{
	/// <summary>
	/// The mode's core concept: notes of different color receive different input. This enum is named "ColorVariant" because
	/// it's designed to let notes of different variant have different colors visually, but each variant doesn't necessarily represent a certain color.
	/// </summary>
	public enum ColorVariant
	{
		/// <summary> Receives input from any key except those of other variants. </summary>
		Gray,

		/// <summary> Receives input from left hand keys. </summary>
		Blue,

		/// <summary> Receives input from right hand keys. </summary>
		Red,

		/// <summary> Receives input from keys of both <see cref="Blue"/> and <see cref="Red"/>. </summary>
		Purple,

		/// <summary> Receives input from some keys not included in <see cref="Purple"/>. </summary>
		Dark
	}
}
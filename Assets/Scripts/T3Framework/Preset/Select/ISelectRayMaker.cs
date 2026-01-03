#nullable enable

using UnityEngine;

namespace T3Framework.Preset.Select
{
	public interface ISelectRayMaker
	{
		Ray GetRay();
	}
}
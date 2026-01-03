#nullable enable

using UnityEngine;

namespace T3Framework.Preset.Select
{
	public class MouseRayMaker : ISelectRayMaker
	{
		private readonly Camera camera;

		public MouseRayMaker(Camera camera) => this.camera = camera;

		public Ray GetRay() => camera.ScreenPointToRay(Input.mousePosition);
	}
}
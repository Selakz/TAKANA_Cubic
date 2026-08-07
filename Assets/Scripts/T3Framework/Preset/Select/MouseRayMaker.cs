#nullable enable

using UnityEngine;
using UnityEngine.InputSystem;

namespace T3Framework.Preset.Select
{
	public class MouseRayMaker : ISelectRayMaker
	{
		private readonly Camera camera;

		public MouseRayMaker(Camera camera) => this.camera = camera;

		public Ray GetRay() => camera.ScreenPointToRay(Mouse.current.position.ReadValue());
	}
}
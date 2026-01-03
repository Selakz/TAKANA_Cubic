#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Basic;
using T3Framework.Preset.Wrapper;
using T3Framework.Runtime.ECS;
using UnityEngine;

namespace MusicGame.ChartEditor.Select
{
	[Serializable]
	public struct TextureAlignInfo
	{
		public string textureName;
		public Vector2 offset;
		public int updateInterval;
		public int colliderLayer;
	}

	public class SelectColliderPlugin : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private BoxCollider boxCollider = default!;

		public BoxColliderModifier ColliderModifier => colliderModifier ??= new BoxColliderModifier(boxCollider);
		private BoxColliderModifier? colliderModifier;

		// Private
		private const float OutWidth = 0.2f;
		private PrefabHandler handler = default!;
		private IT3ModelViewPresenter? presenter;
		private CancellationTokenSource? tokenSource;

		// Defined Functions
		public void StartAligning(TextureAlignInfo alignInfo)
		{
			presenter = handler.Parent!.Script<IT3ModelViewPresenter>();
			var texture = presenter.Textures[alignInfo.textureName].Value;
			tokenSource?.Cancel();
			tokenSource?.Dispose();
			tokenSource = new CancellationTokenSource();
			AlignAsync(tokenSource.Token, alignInfo.updateInterval, alignInfo.offset, texture).Forget();
		}

		private async UniTaskVoid AlignAsync(CancellationToken token, int delay, Vector2 offset, SpriteRenderer texture)
		{
			while (!token.IsCancellationRequested)
			{
				var width = texture.size.x;
				var height = texture.size.y;
				if (offset != Vector2.zero)
				{
					boxCollider.center = new(offset.x * width, offset.y * height, boxCollider.center.z);
				}

				boxCollider.size = new(width + OutWidth, height + OutWidth, boxCollider.size.z);

				if (delay > 0) await UniTask.Delay(delay, cancellationToken: token);
				else await UniTask.DelayFrame(1, cancellationToken: token);
			}
		}

		// System Functions
		void Awake()
		{
			handler = GetComponent<PrefabHandler>();
		}

		void OnEnable()
		{
			transform.localPosition = Vector3.zero;
			boxCollider.center = new(0, 0, boxCollider.center.z);
		}

		void OnDisable()
		{
			tokenSource?.Cancel();
			tokenSource?.Dispose();
			tokenSource = null;
		}
	}
}
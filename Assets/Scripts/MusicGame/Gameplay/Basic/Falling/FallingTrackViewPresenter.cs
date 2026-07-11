#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Wrapper;
using T3Framework.Runtime.Modifier;
using T3Framework.Runtime.Setting;
using T3Framework.Static;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Basic.Falling
{
	public class FallingTrackViewPresenter : MonoBehaviour, INoteViewPresenter, IT3ModelViewPresenter
	{
		// Serializable and Public
		[field: SerializeField]
		public SpriteRendererModifier StartLineModifier { get; private set; } = default!;

		[field: SerializeField]
		public Transform ChunkRoot { get; private set; } = default!;

		public Modifier<Vector2> PositionModifier { get; private set; } = default!;

		public RendererModifier MainTexture => mainModifier;

		public IReadOnlyDictionary<string, RendererModifier> Textures => textures ??=
			new Dictionary<string, RendererModifier>
			{
				["main"] = mainModifier,
				["leftLine"] = leftLineModifier,
				["rightLine"] = rightLineModifier,
				["startLine"] = StartLineModifier
			};

		public IReadOnlyCollection<Modifier<Color>> ColorModifiers => colorModifiers ??=
			new[] { mainModifier.ColorModifier };

		// Private
		private Vector2 Position
		{
			// The track itself is always at the position (0, 0)
			get => ChunkRoot.localPosition;
			set
			{
				Vector3 position = new(value.x, value.y, ChunkRoot.localPosition.z);
				ChunkRoot.localPosition = position;
			}
		}

		private readonly Dictionary<int, TrackChunk> activeChunks = new();
		private readonly ChunkedMeshRendererModifier mainModifier = new();
		private readonly ChunkedMeshRendererModifier leftLineModifier = new();
		private readonly ChunkedMeshRendererModifier rightLineModifier = new();
		private Dictionary<string, RendererModifier>? textures;
		private IReadOnlyCollection<Modifier<Color>>? colorModifiers;

		// Defined Functions
		public void AddChunk(int index, Action<int, TrackChunk> initializeChunk)
		{
			if (!activeChunks.TryGetValue(index, out var chunk))
			{
				var pool = ISingleton<ChunkSpritePoolManager>.Instance;
				chunk = pool.Get(ChunkRoot);
				AttachChunk(chunk);
				activeChunks[index] = chunk;
			}

			initializeChunk.Invoke(index, chunk);
		}

		public void RemoveChunk(int index)
		{
			if (!activeChunks.Remove(index, out var chunk)) return;
			var pool = ISingleton<ChunkSpritePoolManager>.Instance;
			DetachChunk(chunk);
			pool.Release(chunk);
		}

		public void UpdateChunks(Action<int, TrackChunk> initializeChunk)
		{
			foreach (var (index, chunk) in activeChunks) initializeChunk.Invoke(index, chunk);
		}

		public void ClearAllChunks()
		{
			var pool = ISingleton<ChunkSpritePoolManager>.Instance;
			foreach (var (_, chunk) in activeChunks)
			{
				DetachChunk(chunk);
				pool.Release(chunk);
			}

			activeChunks.Clear();
		}

		private void AttachChunk(TrackChunk chunk)
		{
			mainModifier.Attach(chunk.Plane.PlaneRenderer);
			leftLineModifier.Attach(chunk.LeftLine.LineRenderer);
			rightLineModifier.Attach(chunk.RightLine.LineRenderer);
		}

		private void DetachChunk(TrackChunk chunk)
		{
			mainModifier.Detach(chunk.Plane.PlaneRenderer);
			leftLineModifier.Detach(chunk.LeftLine.LineRenderer);
			rightLineModifier.Detach(chunk.RightLine.LineRenderer);
		}

		// System Functions
		[Inject]
		public void BeforeAwake()
		{
			transform.localPosition = Vector3.zero;
			PositionModifier = new Modifier<Vector2>(
				() => Position,
				position => Position = position,
				_ => new(0, ISingletonSetting<PlayfieldSetting>.Instance.UpperThreshold + 1));
		}

		void Update()
		{
			PositionModifier.Update();
			foreach (var cm in ColorModifiers) cm.Update();
		}
	}
}
#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using UnityEngine;

namespace MusicGame.Gameplay.Basic.Falling
{
	public class ChunkSpritePoolManager : MonoBehaviour, ISingletonMonoBehaviour<ChunkSpritePoolManager>
	{
		// Serializable and Public
		[SerializeField] private int prewarmCount = 0;

		public PrefabObject ChunkPrefab { get; set; } = default!;

		// Private
		private readonly Queue<TrackChunk> pool = new();

		// Defined Functions
		public TrackChunk Get(Transform parent)
		{
			TrackChunk chunk;
			if (pool.Count > 0)
			{
				chunk = pool.Dequeue();
				chunk.transform.SetParent(parent, false);
				chunk.gameObject.SetActive(true);
			}
			else
			{
				chunk = ChunkPrefab.SimpleInstantiate(parent).GetComponent<TrackChunk>();
			}

			return chunk;
		}

		public void Release(TrackChunk chunk)
		{
			chunk.Clear();
			chunk.transform.SetParent(transform, false);
			pool.Enqueue(chunk);
		}

		// System Functions
		void Start()
		{
			for (int i = 0; i < prewarmCount; i++)
			{
				var chunk = ChunkPrefab.SimpleInstantiate(transform).GetComponent<TrackChunk>();
				chunk.gameObject.SetActive(false);
				pool.Enqueue(chunk);
			}
		}
	}
}
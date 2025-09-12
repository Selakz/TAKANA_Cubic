using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Components.JudgeLines;
using MusicGame.Components.Movement;
using MusicGame.Components.Tracks.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.MVC;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace MusicGame.Components.Tracks
{
	/// <summary>
	/// Basic track type: only its x-axis moves.
	/// </summary>
	public class Track : ITrack, IControllerRetrievable<TrackController>, IChildOf<IJudgeLine>
	{
		// Public
		public static string TypeMark => "track";

		public int Id { get; set; }

		private static readonly LazyPrefab lazyPrefab = new("Prefabs/Track", "TrackPrefab_OnLoad");
		public GameObject Prefab => lazyPrefab;

		public Vector3 Position { get; set; }

		public T3Time TimeInstantiate
		{
			get => Mathf.Max(timeInstantiate, Parent.TimeInstantiate);
			set => timeInstantiate = value;
		}

		public T3Time TimeEnd { get; set; }

		public float Width { get; set; }

		public TrackController Controller { get; set; } = null;

		MonoBehaviour IControllerRetrievable.Controller => Controller;

		public IJudgeLine Parent { get; set; }

		IComponent IComponent.Parent
		{
			get => Parent;
			set
			{
				if (value is not IJudgeLine judgeLine)
				{
					Debug.LogError($"Track's Parent should be {nameof(IJudgeLine)}");
					return;
				}

				Parent = judgeLine;
			}
		}

		public bool IsPresent => Controller && Controller.Object;

		public ITrackMovement Movement { get; set; }

		public JObject Properties { get; set; }

		// Private
		private T3Time timeInstantiate;

		private static readonly ObjectPool<TrackController> pool = new(
			() => Object.Instantiate<GameObject>(lazyPrefab).GetComponent<TrackController>(),
			null,
			controller => controller.Destroy(),
			controller => Object.Destroy(controller.gameObject));

		// Functions
		public Track(T3Time timeStart, T3Time timeEnd, IJudgeLine parent)
		{
			Id = IComponent.GetUniqueId();
			Parent = parent;
			TimeInstantiate = timeStart;
			TimeEnd = timeEnd;
			Properties = new();
		}

		public bool Generate()
		{
			if (IsPresent) return false;
			Controller = pool.Get();
			Controller.Init(this);
			Controller.Object.SetActive(true);
			return true;
		}

		public bool Destroy()
		{
			if (!IsPresent) return false;
			pool.Release(Controller);
			return true;
		}

		public Track Clone(T3Time timeStart, float xOffset)
		{
			Track track = new(timeStart, timeStart + TimeEnd - TimeInstantiate, Parent)
			{
				Movement = Movement.Clone(timeStart - TimeInstantiate, xOffset)
			};
			return track;
		}

		public JToken GetSerializationToken()
		{
			// ReSharper disable once UseObjectOrCollectionInitializer
			var token = new JObject();
			token.Add("id", Id);
			token.Add("timeStart", TimeInstantiate.Milli);
			token.Add("timeEnd", TimeEnd.Milli);
			token.Add("line", Parent.Id);
			token.Add("movement", Movement.Serialize(true));
			token.AddIf("properties", Properties, Properties.Count > 0);
			return token;
		}

		public static Track Deserialize(JToken token, object context)
		{
			if (token is not JContainer container) return default;
			T3Time timeStart = container["timeStart"]!.Value<int>();
			T3Time timeEnd = container["timeEnd"]!.Value<int>();
			var components = (context as IEnumerable<IComponent>)!;
			IJudgeLine belongingLine =
				components.Where(a => a.Id == container["line"]!.Value<int>()).ToList()[0] as IJudgeLine;

			return new Track(timeStart, timeEnd, belongingLine)
			{
				Id = container["id"]!.Value<int>(),
				Movement = container.TryGetValue("movement", out JToken movementToken)
					? (ITrackMovement)ISerializable.Deserialize(movementToken)
					: new TrackEdgeMovement(Array.Empty<V1EMoveItem>(), Array.Empty<V1EMoveItem>()),
				Properties = container.TryGetValue("properties", out JObject propertiesToken) ? propertiesToken : new()
			};
		}
	}
}
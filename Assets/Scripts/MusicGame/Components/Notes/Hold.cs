using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.Components.Notes.Movement;
using MusicGame.Components.Tracks;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.MVC;
using UnityEngine;
using UnityEngine.Pool;

namespace MusicGame.Components.Notes
{
	public class Hold : BaseNote, IControllerRetrievable<HoldController>
	{
		public static string TypeMark => "hold";

		private static readonly LazyPrefab lazyPrefab = new("Prefabs/Hold", "HoldPrefab_OnLoad");
		public override GameObject Prefab => lazyPrefab;

		public sealed override T3Time TimeEnd
		{
			get => timeEnd;
			set
			{
				timeEnd = value;
				TailMovement.TimeJudge = value;
			}
		}

		public HoldController Controller { get; set; }

		public override bool IsPresent => Controller && Controller.Object.activeSelf;

		public INoteMovement TailMovement { get; set; }

		private T3Time timeEnd;

		private static readonly ObjectPool<HoldController> pool = new(
			() => Object.Instantiate<GameObject>(lazyPrefab).GetComponent<HoldController>(),
			null,
			controller => controller.Destroy(),
			controller => Object.Destroy(controller.gameObject));

		public Hold(T3Time timeJudge, float timeEnd, ITrack belongingTrack)
			: base(timeJudge, belongingTrack)
		{
			this.timeEnd = timeEnd;
			TailMovement = new BaseNoteMoveList(timeEnd);
		}

		public override bool Generate()
		{
			if (IsPresent) return false;
			Controller = pool.Get();
			Controller.Init(this);
			Controller.Object.SetActive(true);
			return true;
		}

		public override bool Destroy()
		{
			if (!IsPresent) return false;
			pool.Release(Controller);
			return true;
		}

		public override BaseNote Clone(T3Time timeJudge, ITrack belongingTrack)
		{
			return new Hold(timeJudge, timeJudge + TimeEnd - TimeJudge, belongingTrack)
			{
				Movement = (INoteMovement)Movement.Clone(timeJudge - TimeJudge, 0),
				TailMovement = (INoteMovement)TailMovement.Clone(timeJudge - TimeJudge, 0),
				Properties = new JObject(Properties)
			};
		}

		public override JToken GetSerializationToken()
		{
			var token = (base.GetSerializationToken() as JObject)!;
			token.Add("timeEnd", TimeEnd.Milli);
			token.AddIf("tailMovement", TailMovement.Serialize(true),
				!(Movement is BaseNoteMoveList baseNoteMoveList && baseNoteMoveList.IsDefault()));
			return token;
		}

		public static Hold Deserialize(JToken token, object context)
		{
			if (token is not JContainer container) return default;
			T3Time timeJudge = container["timeJudge"]!.Value<int>();
			T3Time timeEnd = container["timeEnd"]!.Value<int>();
			var components = (context as IEnumerable<IComponent>)!;
			var component = components.First(a => a.Id == container["track"]!.Value<int>());
			// TODO: Temporary code, fix it by setting Parent property int
			var belongingTrack = component is EditingTrack track ? track.Track : component as ITrack;

			return new Hold(timeJudge, timeEnd, belongingTrack)
			{
				Id = container["id"]!.Value<int>(),
				Movement = container.TryGetValue("movement", out JToken movementToken)
					? (INoteMovement)ISerializable.Deserialize(movementToken)
					: new BaseNoteMoveList(timeJudge),
				TailMovement = container.TryGetValue("movement", out JToken tailMovementToken)
					? (INoteMovement)ISerializable.Deserialize(tailMovementToken)
					: new BaseNoteMoveList(timeEnd),
				Properties = container.TryGetValue("properties", out JObject propertiesToken) ? propertiesToken : new()
			};
		}
	}
}
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.Components.Notes.Movement;
using MusicGame.Components.Tracks;
using MusicGame.Gameplay;
using MusicGame.Gameplay.Level;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.MVC;
using T3Framework.Runtime.Setting;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace MusicGame.Components.Notes
{
	public class Tap : BaseNote, IControllerRetrievable<TapController>
	{
		public static string TypeMark => "tap";

		private static readonly LazyPrefab lazyPrefab = new("Prefabs/Tap", "TapPrefab_OnLoad");
		public override GameObject Prefab => lazyPrefab;

		public TapController Controller { get; set; }

		public override bool IsPresent => Controller && Controller.Object.activeSelf;

		// TODO: Remove pool to a new module
		private static readonly ObjectPool<TapController> pool = new(
			() => Object.Instantiate<GameObject>(lazyPrefab).GetComponent<TapController>(),
			null,
			controller => controller.Destroy(),
			controller => Object.Destroy(controller.gameObject));

		public Tap(T3Time timeJudge, ITrack belongingTrack)
			: base(timeJudge, belongingTrack)
		{
			TimeInstantiate = Mathf.Min(
				Movement.FirstTimeWhen(ISingletonSetting<PlayfieldSetting>.Instance.UpperThreshold, true),
				Movement.FirstTimeWhen(ISingletonSetting<PlayfieldSetting>.Instance.LowerThreshold, false));
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
			var newTap = new Tap(timeJudge, belongingTrack)
			{
				Movement = (INoteMovement)Movement.Clone(timeJudge - TimeJudge, 0),
				Properties = new JObject(Properties)
			};
			return newTap;
		}

		public static Tap Deserialize(JToken token, object context)
		{
			if (token is not JContainer container) return default;
			T3Time timeJudge = container["timeJudge"]!.Value<int>();
			var components = (context as IEnumerable<IComponent>)!;
			var component = components.First(a => a.Id == container["track"]!.Value<int>());
			// TODO: Temporary code, fix it by setting Parent property int
			var belongingTrack = component is EditingTrack track ? track.Track : component as ITrack;

			return new Tap(timeJudge, belongingTrack)
			{
				Id = container["id"]!.Value<int>(),
				Movement = container.TryGetValue("movement", out JToken movementToken)
					? (INoteMovement)ISerializable.Deserialize(movementToken)
					: new BaseNoteMoveList(timeJudge),
				Properties = container.TryGetValue("properties", out JObject propertiesToken) ? propertiesToken : new()
			};
		}
	}
}
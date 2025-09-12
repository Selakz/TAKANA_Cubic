using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.MVC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MusicGame.Components.JudgeLines
{
	public class JudgeLine : IJudgeLine, IControllerRetrievable<JudgeLineController>
	{
		public static string TypeMark => "judgeLine";

		public int Id { get; }

		private static readonly LazyPrefab lazyPrefab = new("Prefabs/JudgeLine", "JudgeLinePrefab_OnLoad");
		public GameObject Prefab => lazyPrefab;

		public T3Time TimeInstantiate { get; internal set; } = T3Time.MinValue;

		public T3Time TimeEnd { get; set; } = T3Time.MaxValue;

		public Quaternion Rotation { get; private set; } = Quaternion.identity;

		public Vector3 Position { get; set; } = new(0, 0, 0);

		public JudgeLineController Controller { get; set; }

		MonoBehaviour IControllerRetrievable.Controller => Controller;

		public bool IsPresent => Controller && Controller.Object.activeSelf;

		public IComponent Parent { get; set; } = null;

		public JObject Properties { get; set; }

		public JudgeLine()
		{
			Id = IComponent.GetUniqueId();
			Properties = new();
		}

		public JudgeLine(float timeInstantiate, Quaternion rotation, Vector3 position)
		{
			Id = IComponent.GetUniqueId();
			TimeInstantiate = timeInstantiate;
			Rotation = rotation;
			Position = position;
		}

		public bool Generate()
		{
			if (Controller != null) return false;
			var go = Object.Instantiate(Prefab);
			go.SetActive(true);
			Controller = go.GetComponent<JudgeLineController>();
			Controller.Init(this);
			return true;
		}

		public bool Destroy()
		{
			if (Controller == null) return false;
			Controller.Destroy();
			return true;
		}

		public JToken GetSerializationToken()
		{
			// ReSharper disable once UseObjectOrCollectionInitializer
			var token = new JObject();
			token.Add("id", Id);
			token.AddIf("timeInstantiate", TimeInstantiate.Milli, TimeInstantiate != T3Time.MinValue);
			token.AddIf("rotation", Rotation.ToString(), Rotation != Quaternion.identity);
			token.AddIf("position", Position.ToString(), Position != Vector3.zero);
			token.AddIf("properties", Properties, Properties.Count > 0);
			return token;
		}

		public static JudgeLine Deserialize(JToken token)
		{
			if (token is not JContainer container) return null;
			var judgeLine = new JudgeLine
			{
				TimeInstantiate = container.Get("timeInstantiate", int.MinValue),
				Rotation = UnityParser.ParseQuaternion(container.Get("rotation", Quaternion.identity.ToString())),
				Position = UnityParser.ParseVector3(container.Get("position", Vector3.zero.ToString())),
				Properties = container.TryGetValue("properties", out JObject propertiesToken) ? propertiesToken : new()
			};
			return judgeLine;
		}
	}
}
using System.Reflection;
using MusicGame.Components;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.MVC;
using UnityEngine;

namespace MusicGame.ChartEditor.EditingComponents
{
	public class EditingComponent : IComponent
	{
		public string TypeMark
		{
			get
			{
				var property = Component.GetType().GetProperty(
					"TypeMark",
					BindingFlags.Public | BindingFlags.Static
				);
				if (property != null && property.PropertyType == typeof(string))
				{
					return (string)property.GetValue(null);
				}

				return "unsupported";
			}
		}

		public IComponent Component { get; private set; }

		MonoBehaviour IControllerRetrievable.Controller => Component.Controller;

		public int Id => Component.Id;

		public GameObject Prefab => Component.Prefab;

		public Vector3 Position
		{
			get => Component.Position;
			set => Component.Position = value;
		}

		public T3Time TimeInstantiate => Component.TimeInstantiate;

		public T3Time TimeEnd
		{
			get => Component.TimeEnd;
			set => Component.TimeEnd = value;
		}

		public bool IsPresent => Component.IsPresent;

		public IComponent Parent
		{
			get => Component.Parent;
			set => Component.Parent = value;
		}

		public JObject Properties { get; set; }

		// Private

		// Static

		// Defined Functions
		public EditingComponent(IComponent component)
		{
			Component = component;
			Properties = new();
		}

		public virtual bool Generate()
		{
			return Component.Generate();
		}

		public virtual bool Destroy()
		{
			return Component.Destroy();
		}

		public virtual JToken GetSerializationToken()
		{
			var token = (Component.GetSerializationToken() as JObject)!;
			token.AddIf("editorconfig", Properties, Properties.Count > 0);
			return token;
		}

		protected static JObject GetEditorConfig(JToken componentToken)
		{
			if (componentToken is not JContainer container) return default;
			return container.TryGetValue("editorconfig", out JObject propertiesToken) ? propertiesToken : new();
		}
	}
}
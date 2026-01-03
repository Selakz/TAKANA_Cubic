#nullable enable

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace T3Framework.Runtime.Serialization.Inspector
{
	[Serializable]
	public struct InspectorType
	{
		[SerializeField] private string abbr;
		[SerializeField] private string typeName;

		private Type? type;
		public Type Type => type ??= Type.GetType(typeName)!;

		[ContextMenu("Fill Type")]
		public void Find()
		{
			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
			foreach (var t in types)
			{
				if (t.Name == abbr)
				{
					typeName = t.AssemblyQualifiedName!;
					Debug.Log($"Found Type: {typeName}");
					break;
				}
			}
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(InspectorType))]
	public class LocalizableTextPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var abbrProp = property.FindPropertyRelative("abbr");
			var typeNameProp = property.FindPropertyRelative("typeName");

			var abbrInput = new TextField
			{
				bindingPath = abbrProp.propertyPath,
				style = { flexGrow = 1 }
			};

			var typeNameInput = new TextField
			{
				bindingPath = typeNameProp.propertyPath,
				style = { flexGrow = 1 }
			};

			var findTypeButton = new Button(() =>
				{
					var keywords = abbrProp.stringValue.Split(' ');
					var abbr = keywords[0];
					var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
					foreach (var t in types)
					{
						if (t.Name == abbr)
						{
							var fullName = t.AssemblyQualifiedName!;
							if (!keywords.All(keyword => fullName.Contains(keyword))) continue;
							typeNameInput.value = fullName;
							Debug.Log($"Found Type: {fullName}");
							return;
						}
					}

					Debug.LogWarning($"Could not find Type: {abbr}");
				})
				{ style = { height = 20 }, text = "Fill Type" };

			var horizontalContainer = new VisualElement { style = { flexDirection = FlexDirection.Column } };
			horizontalContainer.Add(abbrInput);
			horizontalContainer.Add(typeNameInput);
			horizontalContainer.Add(findTypeButton);

			return horizontalContainer;
		}
	}
#endif
}
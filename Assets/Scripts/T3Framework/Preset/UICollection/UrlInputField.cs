#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFB;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Plugins;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Preset.UICollection
{
	public enum PathType
	{
		File,
		Directory,
	}

	public class UrlInputField : T3MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public TMP_InputField InputField { get; set; } = default!;

		[field: SerializeField]
		public Button Button { get; set; } = default!;

		[SerializeField] private PathType type;

		public PathType Type
		{
			get => type;
			set
			{
				type = value;
				Validate(InputField.text);
			}
		}

		[field: SerializeField]
		public I18NText ExplorerTitle { get; set; }

		[field: SerializeField]
		public string InitPath { get; set; } = string.Empty;

		/// <summary> Should manually call <see cref="Validate"/> after modify it. </summary>
		[field: SerializeField]
		public List<ExtensionFilter> Extensions { get; set; } = new();

		public NotifiableProperty<string> Path { get; } = new(string.Empty);

		public NotifiableProperty<bool> IsPathValid { get; } = new(false);

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputFieldRegistrar(InputField, InputFieldRegistrar.RegisterTarget.OnValueChanged, Validate),
			new InputFieldRegistrar(InputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, Validate),
			new ButtonRegistrar(Button, () =>
			{
				switch (Type)
				{
					case PathType.File:
						var filePath = FileBrowser.OpenFileDialog(
							I18NSystem.GetText(ExplorerTitle), InitPath, Extensions.ToArray());
						if (string.IsNullOrEmpty(filePath)) break;
						InputField.text = filePath;
						break;
					case PathType.Directory:
						var folderPath = FileBrowser.OpenFolderDialog(
							I18NSystem.GetText(ExplorerTitle), InitPath);
						if (string.IsNullOrEmpty(folderPath)) break;
						InputField.text = folderPath;
						break;
					default:
						break;
				}
			})
		};

		// Defined Functions
		public void Validate() => Validate(InputField.text);

		private void Validate(string value)
		{
			switch (Type)
			{
				case PathType.File:
					if (!File.Exists(value)) IsPathValid.Value = false;
					else
					{
						IsPathValid.Value = Extensions.Count == 0 ||
						                    Extensions.Any(extension => extension.Extensions.Any(value.EndsWith));
					}

					break;
				case PathType.Directory:
					IsPathValid.Value = Directory.Exists(value);
					break;
				default:
					break;
			}

			Path.Value = value;
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			Validate();
		}
	}
}
#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Chart;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.EditPanel
{
	public class EditNameTitle : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public GameObject SingleIdTitleRoot { get; set; } = default!;

		[field: SerializeField]
		public GameObject CompositeTitleRoot { get; set; } = default!;

		[field: SerializeField]
		public TMP_Text SingleIdText { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField CompositeIdInputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField NameInputField { get; set; } = default!;
	}

	public class NameTitleRegistrar : IEventRegistrar
	{
		private readonly EditNameTitle nameTitle;
		private readonly ChartComponent component;
		private readonly IEventRegistrar[] registrars;
		private CancellationTokenSource? cts;

		public NameTitleRegistrar(EditNameTitle nameTitle, ChartComponent component)
		{
			this.nameTitle = nameTitle;
			this.component = component;
			registrars = new IEventRegistrar[]
			{
				CustomRegistrar.Generic<EventHandler>(
					e => component.OnComponentUpdated += e,
					e => component.OnComponentUpdated -= e,
					(_, _) =>
					{
						nameTitle.SingleIdText.text = component.Id.ToString();
						nameTitle.CompositeIdInputField.SetTextWithoutNotify(component.Id.ToString());
						nameTitle.NameInputField.SetTextWithoutNotify(component.Name ?? string.Empty);
					}),
				new InputFieldRegistrar(nameTitle.CompositeIdInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
					content =>
					{
						if (int.TryParse(content, out int id) && id != component.Id) component.Id = id;
						else nameTitle.CompositeIdInputField.SetTextWithoutNotify(component.Id.ToString());
					}),
				new InputFieldRegistrar(nameTitle.CompositeIdInputField, InputFieldRegistrar.RegisterTarget.OnSelect,
					_ =>
					{
						cts?.Cancel();
						nameTitle.SingleIdTitleRoot.SetActive(false);
						nameTitle.CompositeTitleRoot.SetActive(true);
					}),
				new InputFieldRegistrar(nameTitle.CompositeIdInputField, InputFieldRegistrar.RegisterTarget.OnDeselect,
					_ =>
					{
						cts?.Cancel();
						cts?.Dispose();
						cts = new CancellationTokenSource();
						UniTask.Delay(200, cancellationToken: cts.Token).ContinueWith(UpdateVisible);
					}),
				new InputFieldRegistrar(nameTitle.NameInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
					content =>
					{
						var newName = string.IsNullOrWhiteSpace(content) ? null : content;
						if (newName != component.Name) component.Name = newName;
					}),
				new InputFieldRegistrar(nameTitle.NameInputField, InputFieldRegistrar.RegisterTarget.OnSelect,
					_ =>
					{
						cts?.Cancel();
						nameTitle.SingleIdTitleRoot.SetActive(false);
						nameTitle.CompositeTitleRoot.SetActive(true);
					}),
				new InputFieldRegistrar(nameTitle.NameInputField, InputFieldRegistrar.RegisterTarget.OnDeselect,
					_ =>
					{
						cts?.Cancel();
						cts?.Dispose();
						cts = new CancellationTokenSource();
						UniTask.Delay(200, cancellationToken: cts.Token).ContinueWith(UpdateVisible);
					})
			};
		}

		public void Register()
		{
			nameTitle.SingleIdText.text = component.Id.ToString();
			nameTitle.CompositeIdInputField.SetTextWithoutNotify(component.Id.ToString());
			nameTitle.NameInputField.SetTextWithoutNotify(component.Name ?? string.Empty);
			nameTitle.SingleIdTitleRoot.SetActive(component.Name is null);
			nameTitle.CompositeTitleRoot.SetActive(component.Name is not null);
			foreach (var registrar in registrars) registrar.Register();
		}

		public void Unregister()
		{
			foreach (var registrar in registrars) registrar.Unregister();
		}

		private void UpdateVisible()
		{
			nameTitle.SingleIdTitleRoot.SetActive(component.Name is null);
			nameTitle.CompositeTitleRoot.SetActive(component.Name is not null);
		}
	}
}
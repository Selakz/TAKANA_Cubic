#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class GridWidthUI : HierarchySystem<GridWidthUI>
	{
		// Serializable and Public
		[SerializeField] private Toggle toggle = default!;
		[SerializeField] private Button changeUIButton = default!;

		[SerializeField] private GameObject gridTypeRoot = default!;
		[SerializeField] private TMP_InputField gridIntervalInputField = default!;
		[SerializeField] private TMP_InputField gridOffsetInputField = default!;

		[SerializeField] private GameObject keyTypeRoot = default!;
		[SerializeField] private TMP_InputField keyCountInputField = default!;
		[SerializeField] private TMP_InputField keyPercentInputField = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(retriever.IsOn, value => toggle.SetIsOnWithoutNotify(value)),
			new PropertyRegistrar<float>(retriever.GridInterval, value =>
			{
				gridIntervalInputField.SetTextWithoutNotify(value.ToString("0.000"));
				UpdateKeyUI();
			}),
			new PropertyRegistrar<float>(retriever.GridOffset, value =>
			{
				gridOffsetInputField.SetTextWithoutNotify(value.ToString("0.000"));
				UpdateKeyUI();
			}),

			new ToggleRegistrar(toggle, isOn => retriever.IsOn.Value = isOn),
			new ButtonRegistrar(changeUIButton, () =>
			{
				gridTypeRoot.SetActive(!gridTypeRoot.activeSelf);
				keyTypeRoot.SetActive(!keyTypeRoot.activeSelf);
			}),
			new InputFieldRegistrar(gridIntervalInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, content =>
			{
				if (float.TryParse(content, out float interval)) retriever.GridInterval.Value = interval;
				else gridIntervalInputField.SetTextWithoutNotify(retriever.GridInterval.Value.ToString("0.000"));
			}),
			new InputFieldRegistrar(gridOffsetInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, content =>
			{
				if (float.TryParse(content, out float offset)) retriever.GridOffset.Value = offset;
				else gridOffsetInputField.SetTextWithoutNotify(retriever.GridOffset.Value.ToString("0.000"));
			}),
			new InputFieldRegistrar(keyCountInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, content =>
			{
				if (float.TryParse(content, out float count))
				{
					count = Mathf.Max(count, 1);
					var roundCount = Mathf.RoundToInt(count);
					retriever.GridInterval.Value = KeyBaseWidth / count;
					retriever.GridOffset.Value = Mathf.Abs(count - roundCount) < 1e-3f && roundCount % 2 == 1
						? retriever.GridInterval * 0.5f
						: 0;
				}
				else UpdateKeyUI();
			}),
			new InputFieldRegistrar(keyPercentInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, content =>
			{
				if (int.TryParse(content, out int percent))
					retriever.GridOffset.Value = retriever.GridInterval * (percent / 100f);
				else UpdateKeyUI();
			})
		};

		// Private
		private GridWidthRetriever retriever = default!;

		// Static
		private const float KeyBaseWidth = 9f;

		// Constructor
		[Inject]
		private void Construct(GridWidthRetriever retriever)
		{
			this.retriever = retriever;
		}

		// Defined Functions
		private void UpdateKeyUI()
		{
			var interval = retriever.GridInterval.Value;
			var offset = retriever.GridOffset.Value;
			keyCountInputField.SetTextWithoutNotify((KeyBaseWidth / interval).ToString("0.0"));
			var percent = Mathf.RoundToInt(offset / interval * 100f) % 100;
			keyPercentInputField.SetTextWithoutNotify(percent.ToString());
		}
	}
}
#nullable enable

using MusicGame.ChartEditor.Message;
using T3Framework.Preset.DataContainers;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Record.UI
{
	public class RecordModeButton : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Button button = default!;
		[SerializeField] private Camera playfieldCamera = default!;
		[SerializeField] private Rect viewportRect;
		[SerializeField] private BoolDataContainer isInRecordModeDataContainer = default!;
		[SerializeField] private GameObject editorCanvas = default!;
		[SerializeField] private GameObject fullScreenCanvas = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(button, () =>
			{
				isInRecordModeDataContainer.Property.Value = !isInRecordModeDataContainer.Property.Value;
				var isInRecordMode = isInRecordModeDataContainer.Property.Value;
				editorCanvas.SetActive(!isInRecordMode);
				fullScreenCanvas.SetActive(isInRecordMode);
				playfieldCamera.rect = isInRecordMode ? new Rect(0, 0, 1, 1) : viewportRect;

				if (isInRecordMode) HeaderMessage.Show("鼠标滑至下方以呼出全屏模式UI", HeaderMessage.MessageType.Info);
			})
		};
	}
}
using System;
using TMPro;
using UnityEngine;

public class EditTrackContent : MonoBehaviour
{
	// Serializable and Public
	[SerializeField] private TMP_Text title;
	[SerializeField] private TMP_InputField timeStartInputField;
	[SerializeField] private TMP_InputField timeEndInputField;
	[SerializeField] private TMP_Dropdown layerDropdown;
	[SerializeField] private GameObject leftMoveListScroll;
	[SerializeField] private GameObject rightMoveListScroll;
	[SerializeField] private RectTransform leftMoveListItems;
	[SerializeField] private RectTransform rightMoveListItems;
	[SerializeField] private TMP_Text moveListTitle;

	[Header("Height Control")] [SerializeField]
	private float heightIncrement;

	[SerializeField] private RectTransform backgroundPanel;
	[SerializeField] private RectTransform leftListContent;
	[SerializeField] private RectTransform rightListContent;

	public EditingTrack Track { get; private set; }

	// Private
	private bool isLeftMoveListShow = true;
	private float start;
	private float end;
	private bool isIncremented = false;

	// Static
	private const string prefabPath = "Prefabs/EditorUI/EditTrackContent";
	private static GameObject prefab = null;
	private static bool isFirstShowLeft = true;

	// Defined Functions
	public static EditTrackContent DirectInstantiate(EditingTrack track, RectTransform parent)
	{
		GetPrefab();
		GameObject instance = Instantiate(prefab, parent, false);
		EditTrackContent ret = instance.GetComponent<EditTrackContent>();
		ret.Initialize(track);
		return ret;

		static void GetPrefab()
		{
			if (prefab == null) prefab = MyResources.Load<GameObject>(prefabPath);
		}
	}

	public void Initialize(EditingTrack track)
	{
		Track = track;
		title.text = $"ID: {track.Id}";
		start = track.Track.TimeInstantiate;
		end = track.Track.TimeEnd;
		timeStartInputField.text = Mathf.RoundToInt(start * 1000).ToString();
		timeEndInputField.text = Mathf.RoundToInt(end * 1000).ToString();
		if (TrackLayerManager.Instance != null) layerDropdown.options = TrackLayerManager.Instance.GetOptions();
		layerDropdown.SetValueWithoutNotify(track.Layer);
		RenderMoveList();
		isLeftMoveListShow = isFirstShowLeft;
		DecideListShow();
		DecideHeight();

		if (SelectManager.Instance.SelectedTracks.Count == 1)
		{
			leftMoveListItems.sizeDelta = new(leftMoveListItems.sizeDelta.x, 800);
		}
	}

	// 只负责内容，不负责列表是否展开、选择了什么
	public void RenderMoveList()
	{
		// 左
		for (int i = 0; i < leftMoveListItems.childCount; i++)
		{
			Destroy(leftMoveListItems.GetChild(i).gameObject);
		}

		for (int i = 0; i < Track.Track.LMoveList.Count; i++)
		{
			EditMoveListItem.DirectInstantiate(Track.Track.LMoveList, i, leftMoveListItems);
		}

		// 右
		for (int i = 0; i < rightMoveListItems.childCount; i++)
		{
			Destroy(rightMoveListItems.GetChild(i).gameObject);
		}

		for (int i = 0; i < Track.Track.RMoveList.Count; i++)
		{
			EditMoveListItem.DirectInstantiate(Track.Track.RMoveList, i, rightMoveListItems);
		}
	}

	public void DecideListShow()
	{
		leftMoveListScroll.SetActive(isLeftMoveListShow);
		rightMoveListScroll.SetActive(!isLeftMoveListShow);
		moveListTitle.text = $"运动列表：{(isLeftMoveListShow ? "左" : "右")}边界";
	}

	public void DecideHeight()
	{
		if (SelectManager.Instance.SelectedTracks.Count == 1 && !isIncremented)
		{
			Debug.Log(backgroundPanel.sizeDelta.y);
			backgroundPanel.sizeDelta = new(backgroundPanel.sizeDelta.x, backgroundPanel.sizeDelta.y + heightIncrement);
			Debug.Log(backgroundPanel.sizeDelta.y);
			leftListContent.sizeDelta = new(leftListContent.sizeDelta.x, leftListContent.sizeDelta.y + heightIncrement);
			rightListContent.sizeDelta =
				new(rightListContent.sizeDelta.x, rightListContent.sizeDelta.y + heightIncrement);
			isIncremented = true;
			Debug.Log("incremented");
		}

		if (SelectManager.Instance.SelectedTracks.Count != 1 && isIncremented)
		{
			backgroundPanel.sizeDelta = new(backgroundPanel.sizeDelta.x, backgroundPanel.sizeDelta.y - heightIncrement);
			leftListContent.sizeDelta = new(leftListContent.sizeDelta.x, leftListContent.sizeDelta.y - heightIncrement);
			rightListContent.sizeDelta =
				new(rightListContent.sizeDelta.x, rightListContent.sizeDelta.y - heightIncrement);
			isIncremented = false;
		}
	}

	public void ToggleMoveListShow()
	{
		isLeftMoveListShow = !isLeftMoveListShow;
		DecideListShow();
		isFirstShowLeft = isLeftMoveListShow;
	}

	public void OnUnselectPressed()
	{
		SelectManager.Instance.UnselectTrack(Track.Id);
		EditPanelManager.Instance.AskForRender();
	}

	public void OnDeletePressed()
	{
		CommandManager.Instance.Add(new DeleteTrackCommand(Track.Track));
		EditPanelManager.Instance.AskForRender();
	}

	public void OnStartEndEdit()
	{
		if (int.TryParse(timeStartInputField.text, out int newStartInt))
		{
			if (newStartInt == Mathf.RoundToInt(start * 1000f)) return;
			float newStart = newStartInt / 1000f;
			try
			{
				var command = new UpdateTrackStartCommand(Track.Track, newStart);
				CommandManager.Instance.Add(command);
			}
			catch (Exception)
			{
				HeaderMessage.Show("修改失败", HeaderMessage.MessageType.Warn);
				timeStartInputField.text = Mathf.RoundToInt(start * 1000f).ToString();
			}
		}
		else
		{
			HeaderMessage.Show("修改失败", HeaderMessage.MessageType.Warn);
			timeStartInputField.text = Mathf.RoundToInt(start * 1000f).ToString();
		}
	}

	public void OnEndEndEdit()
	{
		if (int.TryParse(timeEndInputField.text, out int newEndInt))
		{
			if (newEndInt == Mathf.RoundToInt(end * 1000f)) return;
			float newEnd = newEndInt / 1000f;
			try
			{
				var command = new UpdateTrackEndCommand(Track.Track, newEnd);
				CommandManager.Instance.Add(command);
			}
			catch (Exception)
			{
				HeaderMessage.Show("修改失败", HeaderMessage.MessageType.Warn);
				timeStartInputField.text = Mathf.RoundToInt(start * 1000f).ToString();
			}
		}
		else
		{
			HeaderMessage.Show("修改失败", HeaderMessage.MessageType.Warn);
			timeStartInputField.text = Mathf.RoundToInt(start * 1000f).ToString();
		}
	}

	public void OnLayerValueChanged()
	{
		if (layerDropdown.value == 1 && Track.Track.Notes.Count > 0)
		{
			HeaderMessage.Show("该轨道上存在Note，无法切换至装饰层", HeaderMessage.MessageType.Info);
			layerDropdown.SetValueWithoutNotify(Track.Layer);
			return;
		}

		Track.Layer = layerDropdown.value;
		if (TrackLayerManager.Instance != null) Track.OnSelectedLayerChanged(TrackLayerManager.Instance.SelectedLayer);
	}

	// System Functions
}
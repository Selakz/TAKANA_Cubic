using TMPro;
using UnityEngine;
using static Takana3.MusicGame.Values;

public class TimeIndicator : MonoBehaviour, ICanEnableUI
{
    // Serializable and Public
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private RectTransform self;
    [SerializeField] private float pixelPerGameY;

    public float GamePos
    {
        get => self.anchoredPosition.y / pixelPerGameY;
        set
        {
            self.anchoredPosition = new(0, value * pixelPerGameY);
        }
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    // Private

    // Static

    // Defined Functions

    // System Functions
    void Start()
    {

    }

    void Update()
    {
        if (!Camera.main.ContainsScreenPoint(Input.mousePosition))
        {
            GamePos = UpHeightLimit;
        }
        else
        {
            gameObject.SetActive(true);
            var gamePoint = Camera.main.T3ScreenToGamePoint(Input.mousePosition);
            if (GridManager.Instance.IsTGridShow) gamePoint = GridManager.Instance.GetAttachedGamePoint(gamePoint);
            GamePos = gamePoint.y;
            timeText.text = Mathf.RoundToInt(GameYToTime(TimeProvider.Instance.ChartTime, EditingLevelManager.Instance.MusicSetting.Speed, gamePoint.y) * 1000f).ToString();
        }
    }
}

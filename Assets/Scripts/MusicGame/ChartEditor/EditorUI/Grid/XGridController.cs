using UnityEngine;

public class XGridController : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private RectTransform self;
    [SerializeField] private float pixelPerGameX;

    public float GamePos
    {
        get => pos;
        set
        {
            pos = value;
            self.anchoredPosition = new(pos * pixelPerGameX, self.anchoredPosition.y);
        }
    }

    // Private
    private float pos = 0;

    // Static

    // Defined Functions

    // System Functions
}

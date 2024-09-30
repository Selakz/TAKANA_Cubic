using UnityEngine;

// TODO: 学会Shader然后用Shader来描边和高亮。。。
// 目前是专为编辑界面适用的非常sb逻辑
public class Highlight2D : MonoBehaviour
{
    public Sprite highlightSprite;
    public int highlightLayer;

    //private GameObject edge;
    private SpriteRenderer spriteRenderer = null;
    private Sprite originSprite;
    private int originLayer;
    private bool _isHighlight = false;

    public bool IsHighlight
    {
        get => _isHighlight;
        set
        {
            if (_isHighlight == value) return;
            if (spriteRenderer != null)
            {
                if (value)
                {
                    spriteRenderer.sprite = highlightSprite;
                    spriteRenderer.sortingOrder = highlightLayer;
                }
                else
                {
                    spriteRenderer.sprite = originSprite;
                    spriteRenderer.sortingOrder = originLayer;
                }
            }
            _isHighlight = value;
        }
    }

    // System Functions

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originSprite = spriteRenderer.sprite;
        originLayer = spriteRenderer.sortingOrder;
    }
}

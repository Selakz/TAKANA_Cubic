using UnityEngine;

// TODO: ѧ��ShaderȻ����Shader����ߺ͸���������
// Ŀǰ��רΪ�༭�������õķǳ�sb�߼�
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
